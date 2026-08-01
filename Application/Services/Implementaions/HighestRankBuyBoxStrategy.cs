using Application.Dtos.Products.ClientSide;
using Application.Services.Interfaces;

namespace Application.Services.Implementaions
{
    public sealed class HighestRankBuyBoxStrategy : IBuyBoxStrategy
    {
        public void AssignWinners(List<ClientSideScrapedVariantDto> variants)
        {
            foreach (var winner in variants
                         .Where(v => v.IsMarketable)
                         .GroupBy(v => v.ColorName)
                         .Select(g => g.OrderByDescending(v => v.Rank).FirstOrDefault())
                         .Where(w => w is not null))
            {
                winner!.IsBuyBoxWinner = true;
            }
        }
    }
}
