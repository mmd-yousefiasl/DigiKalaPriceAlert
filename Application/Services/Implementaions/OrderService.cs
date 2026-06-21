using Application.Dtos.BalanceTransaction.ClientSide;
using Application.Dtos.Orders.AdminSide;
using Application.Dtos.Orders.ClientSide;
using Application.Dtos.Payment;
using Application.Dtos.Users.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Enums.Orders;
using Domain.Enums.Payment;
using Domain.Enums.Users;
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
        private readonly IBalanceTransactionService _transactionService;
        #endregion

        #region Ctor
        public OrderService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IUsedFreePlanService usedFreePlanService,
            ISubscriptionPlanService subscriptionPlanService,
            IUserService userService,
            IPaymentService paymentService,
            IBalanceTransactionService transactionService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _usedFreePlanService = usedFreePlanService;
            _subscriptionPlanService = subscriptionPlanService;
            _userService = userService;
            _paymentService = paymentService;
            _transactionService = transactionService;
        }
        #endregion

        public async Task<Result<int>> ClientSideCreateNewPurchase(int planId)
        {
            if (planId < 1)
                return Result.Failure<int>(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<int>(Messages.OperationFailedError);

            var planResult = await _subscriptionPlanService.ClientSideGetPlanById(planId);
            if (planResult.IsFailure)
                return Result.Failure<int>(planResult.Message!);

            var plan = planResult.Value;

            var oldOrders = await _context.Orders
                .Where(o => o.UserId == userId && o.Status == OrderStatusEnum.Draft)
                .ToListAsync();

            int finalOrderId = 0;
            bool isExist = false;

            if (oldOrders.Any())
            {
                foreach (var order in oldOrders)
                {
                    if (order.SubscriptionPlanId == planId)
                    {
                        isExist = true;
                        finalOrderId = order.Id;
                    }
                    else
                    {
                        order.Status = OrderStatusEnum.Canceled;
                    }
                }

                _context.Orders.UpdateRange(oldOrders);
            }

            if (!isExist)
            {
                var newOrder = new Order
                {
                    UserId = userId,
                    SubscriptionPlanId = plan.Id,
                    Status = OrderStatusEnum.Draft,
                    PaidDate = null
                };

                await _context.Orders.AddAsync(newOrder);
                await _context.SaveChangesAsync();

                finalOrderId = newOrder.Id;
            }
            else
            {
                await _context.SaveChangesAsync();
            }

            return finalOrderId;
        }

        public async Task<Result<ClientSideInvoiceDto>> ClientSideGetInvoice(int orderId)
        {
            if (orderId < 1)
                return Result.Failure<ClientSideInvoiceDto>(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<ClientSideInvoiceDto>(Messages.OperationFailedError);

            var order = await _context.Orders.Where(n => n.Id == orderId && n.UserId == userId)
                .Select(n => new ClientSideInvoiceDto()
                {
                    Id = n.Id,
                    DurationInDays = n.SubscriptionPlan.DurationInDays,
                    ShortDescription = n.SubscriptionPlan.ShortDescription,
                    IsPopular = n.SubscriptionPlan.IsPopular,
                    PlanPrice = n.SubscriptionPlan.Price,
                    PlanTitle = n.SubscriptionPlan.Name,
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

        public async Task<Result<int>> ClientSideGetInvoiceId()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<int>(Messages.OperationFailedError);

            var invoiceId = await _context.Orders.Where(n => n.UserId == userId && n.Status == OrderStatusEnum.Draft)
                .Select(n => n.Id)
                .FirstOrDefaultAsync();
            if (invoiceId < 1)
                return Result.Failure<int>(Messages.EmptyInvoiceError);

            return invoiceId;
        }

        public async Task<Result<ClientSideProceedToCheckoutDto>> ClientSideProceedToCheckout(bool useBalance)
        {
            var orderIdResult = await ClientSideGetInvoiceId();
            if (orderIdResult.IsFailure)
                return Result.Failure<ClientSideProceedToCheckoutDto>(orderIdResult.Message!);

            var order = await _context.Orders
                .Include(n => n.SubscriptionPlan)
                .FirstOrDefaultAsync(n => n.Id == orderIdResult.Value);

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

                var transactionResult = await _transactionService.ClientSideAddUnVerifiedTransaction(new ClientSideCreateNewTransactionDto()
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

            var verifyTransaction = await _transactionService.ClientSideVerifyTransaction(dto.trackId.ToString());
            if (verifyTransaction.IsFailure)
                return Result.Failure(Messages.PaymentFailedError);

            return Result.Success(Messages.PaymentDoneSuccessfully);
        }

        public async Task<Result<int>> ClientSideVerifyOrder()
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<int>(Messages.OperationFailedError);

            var order = await _context.Orders
                .Include(n => n.SubscriptionPlan)
                .FirstOrDefaultAsync(n => n.UserId == userId && n.Status == OrderStatusEnum.Draft);

            if (order is null)
                return Result.Failure<int>(Messages.OperationFailedError);

            if (order.SubscriptionPlanId == 1)
            {
                var userResult = await _usedFreePlanService.ClientSideCanUserGetFreePlan();
                if (userResult.IsFailure)
                    return Result.Failure<int>(userResult.Message!);
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
                            return Result.Failure<int>(userUsedResult.Message!);
                        }
                    }

                    var withdrawRes = await _userService.ClientSideWithdrawUserOrder(new ClientSideWithdrawUserOrderDto()
                    {
                        Amount = order.SubscriptionPlan.Price,
                        OrderId = order.Id
                    });

                    if (withdrawRes.IsFailure)
                    {
                        await dbTx.RollbackAsync();
                        return Result.Failure<int>(withdrawRes.Message!);
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
                        return Result.Failure<int>(Messages.OperationFailedError);
                    }

                    await _context.SaveChangesAsync();
                    await dbTx.CommitAsync();

                    return order.Id;
                }
                catch (Exception)
                {
                    await dbTx.RollbackAsync();
                    return Result.Failure<int>(Messages.OperationFailedError);
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
