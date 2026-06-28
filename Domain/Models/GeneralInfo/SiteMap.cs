using Domain.Enums.GeneralInfo;

namespace Domain.Models.GeneralInfo
{
    public class SiteMap : AuditBaseEntity<int>
    {
        public string Url { get; set; }
        public ChangeFreqEnum ChangeFreq { get; set; }
        public decimal Priority { get; set; }
        public bool IsPublished { get; set; }
        public SiteMapTypeEnum Type { get; set; }
    }
}
