using Application.Dtos.Orders.AdminSide;
using Application.Dtos.Orders.ClientSide;
using Application.Dtos.Payment;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Result<int>> ClientSideCreateNewPurchase(int planId);

        Task<Result<ClientSideInvoiceDto>> ClientSideGetInvoice(int orderId);

        Task<Result<int>> ClientSideGetInvoiceId();

        Task<Result<ClientSideProceedToCheckoutDto>> ClientSideProceedToCheckout(bool useBalance);

        Task<Result> ClientSideCheckout(PaymentResultDto dto);

        Task<Result<int>> ClientSideVerifyOrder();

        Task<Result<ClientSideFilterOrdersDto>> ClientSideFilterOrders(ClientSideFilterOrdersDto filter);

        Task<Result<AdminSideFilterOrdersDto>> AdminSIdeFilterOrders(AdminSideFilterOrdersDto filter);
    }
}
