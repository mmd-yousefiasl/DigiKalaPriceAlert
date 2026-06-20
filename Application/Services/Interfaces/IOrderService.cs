using Application.Dtos.Orders.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Result<int>> ClientSideCreateNewPurchase(int planId);

        Task<Result<ClientSideInvoiceDto>> ClientSideGetInvoice(int orderId);

        Task<Result<int>> ClientSideGetInvoiceId();
    }
}
