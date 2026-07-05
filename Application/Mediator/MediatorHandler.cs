using MediatR;

namespace Application.Mediator
{
    public class MediatorHandler : IMediatorHandler
    {
        #region Fields
        private readonly IMediator _mediator;
        #endregion

        #region Ctor
        public MediatorHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        #endregion

        public virtual Task PublishEvent<T>(T @event) where T : INotification
        {
            return _mediator.Publish(@event);
        }
    }
}
