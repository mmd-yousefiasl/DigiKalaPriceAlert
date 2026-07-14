namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideProductDto : CommonDto
    {
        public string Title { get; set; }
        public string? ImageUrl { get; set; }
        public string Url { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public bool IsRead { get; set; }

        public decimal LowestPrice { get; set; }
        public decimal HighestPrice { get; set; }

        public int VariantsCount { get; set; }
        public int SellersCount { get; set; }
    }
}
