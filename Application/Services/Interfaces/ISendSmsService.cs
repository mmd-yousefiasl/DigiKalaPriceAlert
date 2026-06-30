using Application.Dtos.SendSms.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface ISendSmsService
    {
        Task<Result> ClientSideSendLoginCode(ClientSideSendLoginCodeDto dto);
    }
}
