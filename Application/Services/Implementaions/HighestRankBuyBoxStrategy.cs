using Application.Dtos.Products.ClientSide;
using Application.Services.Interfaces;

namespace Application.Services.Implementaions
{
    public sealed class HighestRankBuyBoxStrategy : IBuyBoxStrategy
    {
        public void AssignWinners(List<ClientSideScrapedSellerDto> sellers)
        {
            // Group by color — that's where competition actually happens.
            // Use a stable key: ColorId if present, otherwise fall back to hex/name.
            var groups = sellers.GroupBy(s =>
                s.ColorId.HasValue ? $"id:{s.ColorId.Value}" : $"hex:{s.ColorHex}|{s.ColorName}");

            foreach (var group in groups)
            {
                var winner = group.FirstOrDefault();

                if (winner is not null)
                    winner.IsBuyBoxWinner = true;
            }
        }
    }
}
