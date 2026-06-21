using Application.Dtos.Payment;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Result<PaymentRequestResultDto>> Request(PaymentRequestDto model);

        Task<Result<VerifyPaymentResultDto>> VerifyPayment(string transId);
    }
}
