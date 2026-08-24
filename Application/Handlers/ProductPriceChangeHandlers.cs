using Application.Services.Interfaces;
using Domain.Events.Products;
using MediatR;

namespace Application.Handlers
{
    public class ProductPriceChangeHandlers : INotificationHandler<ProductPriceChangedEvent>
    {
        #region Fields
        private readonly IUserService _userService;
        #endregion

        #region Ctor
        public ProductPriceChangeHandlers(IUserService userService)
        {
            _userService = userService;
        }
        #endregion
        public Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
