using Application.Dtos.ContactUs.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IContactUsFormService
    {
        Task<Result> ClientSideSendNewContactUsForm(ClientSideSendNewContactUsFormDto dto);
    }
}
