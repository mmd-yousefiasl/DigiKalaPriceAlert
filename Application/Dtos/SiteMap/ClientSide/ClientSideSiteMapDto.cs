using Domain.Enums.GeneralInfo;

namespace Application.Dtos.SiteMap.ClientSide
{
    public class ClientSideSiteMapDto
    {
        public DateTime LastUpdateDate { get; set; }
        public string Url { get; set; }
        public ChangeFreqEnum ChangeFreq { get; set; }
        public decimal Priority { get; set; }
    }
}
