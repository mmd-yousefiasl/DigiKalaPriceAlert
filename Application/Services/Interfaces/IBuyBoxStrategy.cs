using Application.Dtos.Products.ClientSide;

namespace Application.Services.Interfaces
{
    public interface IBuyBoxStrategy
    {
        void AssignWinners(List<ClientSideScrapedSellerDto> sellers);
    }
}
