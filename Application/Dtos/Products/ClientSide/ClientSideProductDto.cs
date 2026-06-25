namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideProductDto : CommonDto
    {
        public string Title { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public decimal NewPrice { get; set; }
        public decimal OldPrice { get; set; }
        public decimal? ChangePercent { get; set; }
        public string Url { get; set; }
        public bool IsRead { get; set; }
    }
}
