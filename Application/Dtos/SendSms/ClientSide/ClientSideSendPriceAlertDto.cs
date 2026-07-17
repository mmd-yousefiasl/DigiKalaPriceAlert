using Domain.Enums.GeneralInfo;

namespace Application.Dtos.SendSms.ClientSide
{
    public class ClientSideSendPriceAlertDto
    {
        public SmsPanelTypeEnum Type { get; set; }
        public string Mobile { get; set; }
        public string FirstName { get; set; }
    }
}
