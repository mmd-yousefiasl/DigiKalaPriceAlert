using Application.Dtos.SendSms.ClientSide;
using Application.Dtos.SmsConsumer;
using Application.Services.Interfaces;
using Application.Tools;
using Domain.Enums.GeneralInfo;
using Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementaions
{
    public class SendSmsService : ISendSmsService
    {
        #region Fields
        private readonly ISmsPanelService _smsPanelService;
        private readonly ILogger<ISendSmsService> _logger;
        private readonly ISmsQueue _smsQueue;
        #endregion

        #region Ctor
        public SendSmsService(ISmsPanelService smsPanelService,
            ILogger<ISendSmsService> logger,
            ISmsQueue smsQueue)
        {
            _smsPanelService = smsPanelService;
            _logger = logger;
            _smsQueue = smsQueue;
        }
        #endregion

        public async Task<Result> ClientSideSendLoginCode(ClientSideSendLoginCodeDto dto)
        {
            if (dto is null)
                return Result.Failure(Messages.OperationFailedError);

            var infoResult = await _smsPanelService.ClientSideGetSmsPanel(SmsPanelTypeEnum.OTP);
            if (infoResult.IsFailure)
                return Result.Failure(Messages.OperationFailedError);

            var loginSms = new SmsQueueItem(
                Mobile: dto.Mobile,
                ApiKey: infoResult.Value.ApiKey,
                PatternId: infoResult.Value.PatternId,
                Parameters: new List<SmsParameterItem>
                {
                        new("OTP", dto.Code)
                }
            );

            await _smsQueue.QueueSmsAsync(loginSms);

            return Result.Success();
        }

        public async Task<Result> ClientSideSendPriceAlert(ClientSideSendPriceAlertDto dto)
        {
            if (dto is null)
                return Result.Failure(Messages.OperationFailedError);

            var infoResult = await _smsPanelService.ClientSideGetSmsPanel(dto.Type);
            if (infoResult.IsFailure)
                return Result.Failure(Messages.OperationFailedError);

            var firstName = dto.FirstName.ToSafeSmsName();

            var loginSms = new SmsQueueItem(
                Mobile: dto.Mobile,
                ApiKey: infoResult.Value.ApiKey,
                PatternId: infoResult.Value.PatternId,
                Parameters: new List<SmsParameterItem>
                {
                    new("NAME", firstName)
                }
            );

            await _smsQueue.QueueSmsAsync(loginSms);

            return Result.Success();
        }
    }
}