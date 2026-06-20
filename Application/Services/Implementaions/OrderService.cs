using Application.Dtos.Orders.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Enums.Orders;
using Domain.Models.Orders;
using Domain.Shared;
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
        #endregion

        #region Ctor
        public OrderService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IUsedFreePlanService usedFreePlanService,
            ISubscriptionPlanService subscriptionPlanService,
            IUserService userService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _usedFreePlanService = usedFreePlanService;
            _subscriptionPlanService = subscriptionPlanService;
            _userService = userService;
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
                .Where(o => o.UserId == userId &&
                           (o.Status == OrderStatusEnum.Draft || o.Status == OrderStatusEnum.Pending))
                .ToListAsync();

            if (oldOrders.Any())
            {
                foreach (var order in oldOrders)
                {
                    order.Status = OrderStatusEnum.Canceled;
                }
                _context.Orders.UpdateRange(oldOrders);
            }

            var newOrder = new Order
            {
                UserId = userId,
                SubscriptionPlanId = plan.Id,
                PlanPrice = plan.Price,
                PlanTitle = $"خرید اشتراک {plan.Name}",
                Status = OrderStatusEnum.Draft,
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            return Result.Success(newOrder.Id);
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

            var userBalance = await _userService.ClientSideGetCurrentUserBalance();
            if (userBalance.IsSuccess)
                order.UserBalance = userBalance.Value;

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
    }
}
