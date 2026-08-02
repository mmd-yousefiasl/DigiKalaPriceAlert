using Application.Dtos.Products.ClientSide;
using Application.Services.Interfaces;

namespace Application.Services.Implementaions
{
    public sealed class HighestRankBuyBoxStrategy : IBuyBoxStrategy
    {
        public void AssignWinners(List<ClientSideScrapedSellerDto> sellers)
        {
            var groups = sellers.GroupBy(s =>
                s.ColorId.HasValue ? $"id:{s.ColorId.Value}" : $"hex:{s.ColorHex}|{s.ColorName}");

            foreach (var group in groups)
            {
                var winner = group.FirstOrDefault(s => s.Status == "marketable");

                if (winner is not null)
                    winner.IsBuyBoxWinner = true;
            }
        }
    }
}
