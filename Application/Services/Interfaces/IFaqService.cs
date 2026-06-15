using Application.Dtos.Faq.AdminSide;
using Application.Dtos.Faq.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IFaqService
    {
        Task<Result<AdminSideFilterFaqsDto>> FilterFaqs(AdminSideFilterFaqsDto filter);

        Task<Result> CreateNewFaq(AdminSideCreateNewFaqDto model);

        Task<Result<AdminSideUpdateFaqDto>> GetFaqForUpdate(int id);

        Task<Result> UpdateFaq(AdminSideUpdateFaqDto model);

        Task<Result> SoftDeleteFaq(int id);

        Task<Result<List<ClientSideFaqDto>>> GetFaqs();
    }
}
