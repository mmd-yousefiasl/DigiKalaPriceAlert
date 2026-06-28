using Domain.Enums.GeneralInfo;

namespace Application.Dtos.SiteMap.AdminSide
{
    public class AdminSideSiteMapDto : CommonDto
    {
        public string Url { get; set; }
        public ChangeFreqEnum ChangeFreq { get; set; }
        public decimal Priority { get; set; }
        public bool IsPublished { get; set; }
        public SiteMapTypeEnum Type { get; set; }
    }
}
