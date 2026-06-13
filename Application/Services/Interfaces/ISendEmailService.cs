using Application.Dtos.SendEmail.AdminSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface ISendEmailService
    {
        Task<Result> AdminSideSendBuckEmail(AdminSideSendBuckEmailDto dto);
    }
}
