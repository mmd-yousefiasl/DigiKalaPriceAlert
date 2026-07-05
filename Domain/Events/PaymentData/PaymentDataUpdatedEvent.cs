using MediatR;

namespace Domain.Events.PaymentData
{
    public record PaymentDataUpdatedEvent(
        int Id,
        string PaymentGatewayPin,
        string? FileName,
        string? Url
        ) : INotification;
}
