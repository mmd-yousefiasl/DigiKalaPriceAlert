using Application.Dtos.PaymentData;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IPaymentDataService
    {
        Task<Result<AdminSideUpdatePaymentDataDto>> AdminSideGetPaymentDataForUpdate();

        Task<Result> AdminSideUpdatePaymentData(AdminSideUpdatePaymentDataDto model);

        Task<Result<string>> GetPin();

        Task<Result<ClientSidePaymentDataLogoDto>> ClientSideGetPaymentDataLogo();
    }
}
