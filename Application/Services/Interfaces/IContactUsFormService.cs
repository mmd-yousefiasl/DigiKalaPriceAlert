using Application.Dtos.ContactUs.AdminSide;
using Application.Dtos.ContactUs.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IContactUsFormService
    {
        Task<Result> ClientSideSendNewContactUsForm(ClientSideSendNewContactUsFormDto dto);

        Task<Result<AdminSideFilterContactUsFormsDto>> AdminSideFilterContactUsFomrs(AdminSideFilterContactUsFormsDto filter);

        Task<Result> AdminSideDeleteContactUsForm(int id);

        Task<Result> AdminSideCheckContactUsFormAsRead(int id);
    }
}
