using Application.Services.Interfaces;
using Data.Context;

namespace Application.Services.Implementaions
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        #endregion

        #region Ctor
        public SubscriptionPlanService(DigiKalaPriceAlertDbContext context)
        {
            _context = context;
        }
        #endregion
    }
}
