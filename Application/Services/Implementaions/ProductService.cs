using Application.Services.Interfaces;
using Data.Context;

namespace Application.Services.Implementaions
{
    public class ProductService : IProductService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        #endregion

        #region Ctor
        public ProductService(DigiKalaPriceAlertDbContext context)
        {
            _context = context;
        }
        #endregion


    }
}
