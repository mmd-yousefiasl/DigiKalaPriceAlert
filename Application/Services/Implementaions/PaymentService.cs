using Application.Dtos.Payment;
using Application.Services.Interfaces;
using Domain.Shared;
using Domain.Statics;
using System.Net.Http.Json;

namespace Application.Services.Implementaions
{
    public class PaymentService : IPaymentService
    {
        #region Fields
        private readonly IPaymentDataService _paymentDataService;
        #endregion

        #region Ctor
        public PaymentService(IPaymentDataService paymentDataService)
        {
            _paymentDataService = paymentDataService;
        }
        #endregion

        public async Task<Result<PaymentRequestResultDto>> Request(PaymentRequestDto model)
        {
            if (model is null) return Result.Failure<PaymentRequestResultDto>(Messages.OperationFailedError);

            var pinResult = await _paymentDataService.GetPin();
            if (pinResult.IsFailure) return Result.Failure<PaymentRequestResultDto>(pinResult.Message);

            var requestData = new
            {
                merchant = pinResult.Value,
                amount = model.Amount,
                callbackUrl = model.CallBackUrl
            };

            using var client = new HttpClient();

            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(SiteTools.PaymentRequestURL, requestData);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<PaymentRequestResultDto>();
                    return result;
                }
                else
                {
                    return Result.Failure<PaymentRequestResultDto>(Messages.OperationFailedError);
                }
            }
            catch
            {
                return Result.Failure<PaymentRequestResultDto>(Messages.OperationFailedError);
            }
        }

        public async Task<Result<VerifyPaymentResultDto>> VerifyPayment(string transId)
        {
            if (string.IsNullOrWhiteSpace(transId)) return Result.Failure<VerifyPaymentResultDto>(Messages.OperationFailedError);

            var pinResult = await _paymentDataService.GetPin();
            if (pinResult.IsFailure) return Result.Failure<VerifyPaymentResultDto>(pinResult.Message);

            var requestData = new
            {
                merchant = pinResult.Value,
                trackId = transId
            };

            using var client = new HttpClient();

            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(SiteTools.VerifyPaymentURL, requestData);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<VerifyPaymentResultDto>();
                    return result;
                }
                else
                {
                    return Result.Failure<VerifyPaymentResultDto>(Messages.OperationFailedError);
                }
            }
            catch
            {
                return Result.Failure<VerifyPaymentResultDto>(Messages.OperationFailedError);
            }
        }
    }
}
