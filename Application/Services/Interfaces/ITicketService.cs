using Application.Dtos.Tickets.ClientSide;
using Domain.Shared;

namespace Application.Services.Interfaces
{
    public interface ITicketService
    {
        Task<Result<int>> ClientSideCreateNewTicket(ClientSideCreateNewTicketDto dto);
        Task<Result<ClientSideFilterTicketsDto>> ClientSideGetUserTickets(ClientSideFilterTicketsDto filter);
        Task<Result<ClientSideTicketDetailsDto>> ClientSideGetTicketDetails(int ticketId);
        Task<Result> ClientSideCloseTicket(int ticketId);

        Task<Result> ClientSideSendNewMessage(ClientSideSendTicketMessageDto dto);
    }
}
