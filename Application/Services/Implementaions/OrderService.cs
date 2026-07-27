using Application.Dtos.BalanceTransaction.ClientSide;
using Application.Dtos.Orders.AdminSide;
using Application.Dtos.Orders.ClientSide;
using Application.Dtos.Payment;
using Application.Dtos.Users.ClientSide;
using Application.Mediator;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Enums.Orders;
using Domain.Enums.Payment;
using Domain.Enums.Users;
using Domain.Events.Orders;
using Domain.Models.Orders;
using Domain.Shared;
using Domain.Statics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class OrderService : IOrderService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUsedFreePlanService _usedFreePlanService;
        private readonly ISubscriptionPlanService _subscriptionPlanService;
        private readonly IUserService _userService;
        private readonly IPaymentService _paymentService;
        private readonly IMediatorHandler _mediator;
        #endregion

        #region Ctor
        public OrderService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IUsedFreePlanService usedFreePlanService,
            ISubscriptionPlanService subscriptionPlanService,
            IUserService userService,
            IPaymentService paymentService,
            IMediatorHandler mediator)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _usedFreePlanService = usedFreePlanService;
            _subscriptionPlanService = subscriptionPlanService;
            _userService = userService;
            _paymentService = paymentService;
            _mediator = mediator;
        }
        #endregion

        public async Task<Result<string>> ClientSideCreateNewPurchase(int planId)
        {
            // 1. Fail-fast validation guards
            if (planId < 1)
                return Result.Failure<string>(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<string>(Messages.OperationFailedError);

            var planResult = await _subscriptionPlanService.ClientSideGetPlanById(planId);
            if (planResult.IsFailure)
                return Result.Failure<string>(planResult.Message!);

            var plan = planResult.Value;

            // 2. Check if the EXACT draft order already exists
            var existingOrderInvoiceId = await _context.Orders
                .Where(o => o.UserId == userId && o.SubscriptionPlanId == planId && o.Status == OrderStatusEnum.Draft)
                .Select(o => o.InvoiceId)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(existingOrderInvoiceId))
            {
                // Cancel ANY OTHER draft plans the user had open except this current one
                await _context.Orders
                    .Where(o => o.UserId == userId && o.SubscriptionPlanId != planId && o.Status == OrderStatusEnum.Draft)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(o => o.Status, OrderStatusEnum.Canceled));

                return existingOrderInvoiceId;
            }

            // 3. Cancel all old drafts instantly in 1 single database roundtrip (No EF tracking needed)
            await _context.Orders
                .Where(o => o.UserId == userId && o.Status == OrderStatusEnum.Draft)
                .ExecuteUpdateAsync(setters => setters.SetProperty(o => o.Status, OrderStatusEnum.Canceled));

            // 4. Generate Unique Invoice ID using a rapid verification set
            string uniqueInvoiceId = string.Empty;
            bool isDuplicate = true;
            for (int i = 0; i < 3; i++)
            {
                uniqueInvoiceId = Generate.InvoiceId();
                if (!await _context.Orders.AnyAsync(o => o.InvoiceId == uniqueInvoiceId))
                {
                    isDuplicate = false;
                    break;
                }
            }

            if (isDuplicate)
                return Result.Failure<string>(Messages.FailedToCreatePurchaseError);

            // 5. Create and attach the new Draft record
            var newOrder = new Order
            {
                UserId = userId,
                SubscriptionPlanId = plan.Id,
                Status = OrderStatusEnum.Draft,
                PaidDate = null,
                InvoiceId = uniqueInvoiceId
            };

            await _context.Orders.AddAsync(newOrder);
            await _context.SaveChangesAsync();

            return uniqueInvoiceId;
        }

        public async Task<Result<ClientSideInvoiceDto>> ClientSideGetInvoice(string invoiceId)
        {
            if (string.IsNullOrWhiteSpace(invoiceId))
                return Result.Failure<ClientSideInvoiceDto>(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<ClientSideInvoiceDto>(Messages.OperationFailedError);

            var order = await _context.Orders.Where(n => n.InvoiceId == invoiceId && n.UserId == userId)
                .Select(n => new ClientSideInvoiceDto()
                {
                    DurationInDays = n.SubscriptionPlan.DurationInDays,
                    ShortDescription = n.SubscriptionPlan.ShortDescription,
                    IsPopular = n.SubscriptionPlan.IsPopular,
                    PlanPrice = n.SubscriptionPlan.Price,
                    PlanTitle = n.SubscriptionPlan.Name,
                    InvoiceId = n.InvoiceId,
                    Features = n.SubscriptionPlan.Features.Where(f => f.IsPublished).OrderBy(f => f.Priority)
                    .Select(f => f.Title).ToList(),
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (order is null)
                return Result.Failure<ClientSideInvoiceDto>(Messages.OperationFailedError);

            order.UserBalance = await _userService.ClientSideGetCurrentUserBalance();

            return order;
        }

        public async Task<Result<string>> ClientSideGetInvoiceId()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<string>(Messages.OperationFailedError);

            var invoiceId = await _context.Orders.Where(n => n.UserId == userId && n.Status == OrderStatusEnum.Draft)
                .Select(n => n.InvoiceId)
                .FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(invoiceId))
                return Result.Failure<string>(Messages.EmptyInvoiceError);

            return invoiceId;
        }

        public async Task<Result<ClientSideProceedToCheckoutDto>> ClientSideProceedToCheckout(bool useBalance)
        {
            var orderIdResult = await ClientSideGetInvoiceId();
            if (orderIdResult.IsFailure)
                return Result.Failure<ClientSideProceedToCheckoutDto>(orderIdResult.Message!);

            var order = await _context.Orders
                .Include(n => n.SubscriptionPlan)
                .FirstOrDefaultAsync(n => n.InvoiceId == orderIdResult.Value);

            if (order is null)
                return Result.Failure<ClientSideProceedToCheckoutDto>(Messages.OperationFailedError);

            if (order.SubscriptionPlanId == 1)
            {
                var userResult = await _usedFreePlanService.ClientSideCanUserGetFreePlan();
                if (userResult.IsFailure)
                    return Result.Failure<ClientSideProceedToCheckoutDto>(userResult.Message!);

                return new ClientSideProceedToCheckoutDto()
                {
                    IsDone = true
                };
            }

            var userBalance = await _userService.ClientSideGetCurrentUserBalance();
            var planPrice = order.SubscriptionPlan.Price;
            decimal requiredGatewayAmount = planPrice;

            if (useBalance && userBalance > 0)
            {
                if (userBalance >= planPrice)
                {
                    requiredGatewayAmount = 0;
                }
                else
                {
                    requiredGatewayAmount = planPrice - userBalance;
                }
            }

            if (requiredGatewayAmount > 0)
            {
                var paymentResult = await _paymentService.Request(new PaymentRequestDto
                {
                    Amount = requiredGatewayAmount * 10,
                    CallBackUrl = SiteTools.SiteUrl + SiteTools.CallBackURL
                });

                if (paymentResult.IsFailure)
                    return Result.Failure<ClientSideProceedToCheckoutDto>(paymentResult.Message!);

                var transactionResult = await _userService.ClientSideAddUnVerifiedTransaction(new ClientSideCreateNewTransactionDto()
                {
                    Amount = requiredGatewayAmount,
                    TransId = paymentResult.Value.trackId.ToString(),
                    IsSuccess = false,
                    Type = BalanceTransactionTypeEnum.Credit,
                    Description = Messages.IncreaseUserBalanceMessage
                });

                if (transactionResult.IsFailure)
                    return Result.Failure<ClientSideProceedToCheckoutDto>(transactionResult.Message!);

                return Result.Success(new ClientSideProceedToCheckoutDto()
                {
                    IsDone = false,
                    TransId = paymentResult.Value.trackId.ToString()
                });
            }
            else
            {
                return Result.Success(new ClientSideProceedToCheckoutDto()
                {
                    IsDone = true
                });
            }
        }

        public async Task<Result> ClientSideCheckout(PaymentResultDto dto)
        {
            if (dto is null)
                return Result.Failure(Messages.OperationFailedError);

            if (dto.status != PaymentResultStatusEnum.PaidButNotVerified)
                return Result.Failure(Messages.PaymentFailedError);

            var paymentResult = await _paymentService.VerifyPayment(dto.trackId.ToString());
            if (paymentResult.IsFailure || paymentResult.Value.result != VerifyPaymentResultStatusEnum.Success)
                return Result.Failure(Messages.PaymentFailedError);

            var verifyTransaction = await _userService.ClientSideVerifyTransaction(dto.trackId.ToString());
            if (verifyTransaction.IsFailure)
                return Result.Failure(Messages.PaymentFailedError);

            return Result.Success(Messages.PaymentDoneSuccessfully);
        }

        public async Task<Result<string>> ClientSideVerifyOrder()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<string>(Messages.OperationFailedError);

            var order = await _context.Orders
                .Include(n => n.SubscriptionPlan)
                .FirstOrDefaultAsync(n => n.UserId == userId && n.Status == OrderStatusEnum.Draft);

            if (order is null)
                return Result.Failure<string>(Messages.OperationFailedError);

            if (order.SubscriptionPlanId == 1)
            {
                var userResult = await _usedFreePlanService.ClientSideCanUserGetFreePlan();
                if (userResult.IsFailure)
                    return Result.Failure<string>(userResult.Message!);
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var dbTx = await _context.Database.BeginTransactionAsync();
                try
                {
                    if (order.SubscriptionPlanId == 1)
                    {
                        var userUsedResult = await _usedFreePlanService.ClientSideCreateNewUsedFreePlan();
                        if (userUsedResult.IsFailure)
                        {
                            await dbTx.RollbackAsync();
                            return Result.Failure<string>(userUsedResult.Message!);
                        }
                    }

                    var withdrawRes = await _userService.ClientSideWithdrawUserOrder(new ClientSideWithdrawUserOrderDto()
                    {
                        Amount = order.SubscriptionPlan.Price,
                        InvoiceId = order.InvoiceId
                    });

                    if (withdrawRes.IsFailure)
                    {
                        await dbTx.RollbackAsync();
                        return Result.Failure<string>(withdrawRes.Message!);
                    }

                    order.PaidDate = DateTime.UtcNow;
                    order.PlanPrice = order.SubscriptionPlan.Price;
                    order.PlanTitle = order.SubscriptionPlan.Name;
                    order.Status = OrderStatusEnum.Success;
                    _context.Orders.Update(order);

                    var userSubResult = await _userService.ClientSideAddToUserSubscription(order.SubscriptionPlan.DurationInDays);
                    if (userSubResult.IsFailure)
                    {
                        await dbTx.RollbackAsync();
                        return Result.Failure<string>(Messages.OperationFailedError);
                    }

                    await _context.SaveChangesAsync();
                    await dbTx.CommitAsync();

                    OrderUpdatedEvent OrderUpdatedEvent = new OrderUpdatedEvent(
                    order.Id,
                    order.CreatedDate,
                    order.LastUpdatedDate,
                    order.Status,
                    order.PlanPrice,
                    order.PlanTitle,
                    order.PaidDate,
                    order.InvoiceId,
                    order.SubscriptionPlanId,
                    order.UserId);

                    await _mediator.PublishEvent(OrderUpdatedEvent);

                    return order.InvoiceId;
                }
                catch (Exception)
                {
                    await dbTx.RollbackAsync();
                    return Result.Failure<string>(Messages.OperationFailedError);
                }
            });
        }

        public async Task<Result<ClientSideFilterOrdersDto>> ClientSideFilterOrders(ClientSideFilterOrdersDto filter)
        {
            if (filter is null)
                return Result.Failure<ClientSideFilterOrdersDto>(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<ClientSideFilterOrdersDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<Order>();

            conditions.Add(n => n.UserId == userId);
            conditions.Add(n => n.Status == OrderStatusEnum.Success);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.PlanTitle.Contains(filter.Search));
            }

            IQueryable<Order> query = _context.Orders;

            query = query.OrderByDescending(n => n.Id);

            foreach (var item in conditions ?? new FilterConditions<Order>())
            {
                query = query.Where(item);
            }

            await filter.Paging(query.Select(n => new ClientSideOrderDto()
            {
                Id = n.Id,
                PaidDate = n.PaidDate,
                PlanPrice = n.PlanPrice,
                PlanTitle = n.PlanTitle,
                InvoiceId = n.InvoiceId
            }));

            return filter;
        }

        public async Task<Result<AdminSideFilterOrdersDto>> AdminSIdeFilterOrders(AdminSideFilterOrdersDto filter)
        {
            if (filter is null)
                return Result.Failure<AdminSideFilterOrdersDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<Order>();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.SubscriptionPlan.Name.Contains(filter.Search) ||
                n.User.Mobile == filter.Search);
            }

            if (filter.Status is not null)
            {
                conditions.Add(n => n.Status == filter.Status);
            }

            IQueryable<Order> query = _context.Orders;

            query = query.OrderByDescending(n => n.Id);

            foreach (var item in conditions ?? new FilterConditions<Order>())
            {
                query = query.Where(item);
            }

            await filter.Paging(query.Select(n => new AdminSideOrderDto()
            {
                Id = n.Id,
                UserId = n.UserId,
                Status = n.Status,
                PaidDate = n.PaidDate,
                PlanPrice = n.PlanPrice,
                PlanTitle = n.SubscriptionPlan.Name,
                UserMobile = n.User.Mobile
            }));

            return filter;
        }
    }
}
