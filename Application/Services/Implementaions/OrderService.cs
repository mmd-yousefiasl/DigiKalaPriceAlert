using Application.Services.Interfaces;
using Data.Context;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Implementaions
{
    public class OrderService : IOrderService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUsedFreePlanService _usedFreePlanService;
        #endregion

        #region Ctor
        public OrderService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IUsedFreePlanService usedFreePlanService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _usedFreePlanService = usedFreePlanService;
        }
        #endregion


    }
}
