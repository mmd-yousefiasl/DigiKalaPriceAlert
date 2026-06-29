using Domain.Enums.GeneralInfo;

namespace Application.Dtos.SmsPanel.AdminSide
{
    public class AdminSideSmsPanelDto : CommonDto
    {
        public SmsPanelTypeEnum Type { get; set; }
        public bool IsActive { get; set; }
    }
}
