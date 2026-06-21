using Application.Dtos.Tickets.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Enums.Tickets;
using Domain.Models.Tickets;
using Domain.Shared;
using Domain.Statics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class TicketService : ITicketService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Ctor
        public TicketService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        public async Task<Result<int>> ClientSideCreateNewTicket(ClientSideCreateNewTicketDto dto)
        {
            if (dto is null)
                return Result.Failure<int>(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext?.User.GetUserId() ?? 0;
            if (userId < 1)
                return Result.Failure<int>(Messages.OperationFailedError);

            var ticket = new Ticket()
            {
                Title = dto.Title,
                Status = TicketStatusEnum.Open,
                Category = dto.Category,
                Priority = dto.Priority,
                UserId = userId,
                Messages = new List<TicketMessage>()
            };

            var ticketMessage = new TicketMessage()
            {
                IsAdminResponse = false,
                SenderId = userId,
                Text = dto.MessageText
            };

            string? fileName = null;
            if (dto.Attachment is not null)
            {
                if (!dto.Attachment.IsImage())
                    return Result.Failure<int>(Messages.InvalidFileFormatError);

                if (dto.Attachment.Length > SiteTools.MaxTicketAttachmentLength.CalculateKB())
                    return Result.Failure<int>(Messages.MaxLengthError);

                fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(dto.Attachment.FileName);
                ticketMessage.AttachmentPath = fileName;
            }

            ticket.Messages.Add(ticketMessage);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (dto.Attachment is not null && fileName is not null)
                {
                    dto.Attachment.UploadSensitiveFileToServer(fileName, SiteTools.TicketAttachmentFile);
                }

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return ticket.Id;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                if (dto.Attachment is not null && fileName is not null)
                {
                    fileName.RemoveSensitiveFileFromServer(SiteTools.TicketAttachmentFile);
                }

                return Result.Failure<int>(Messages.OperationFailedError);
            }
        }

        public async Task<Result<ClientSideFilterTicketsDto>> ClientSideGetUserTickets(ClientSideFilterTicketsDto filter)
        {
            if (filter is null)
                return Result.Failure<ClientSideFilterTicketsDto>(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1) return Result.Failure<ClientSideFilterTicketsDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<Ticket>();

            conditions.Add(n => n.UserId == userId);
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.Title.Contains(filter.Search));
            }

            if (filter.Status is not null)
            {
                conditions.Add(n => n.Status == filter.Status);
            }

            if (filter.Category is not null)
            {
                conditions.Add(n => n.Category == filter.Category);
            }

            if (filter.Priority is not null)
            {
                conditions.Add(n => n.Priority == filter.Priority);
            }

            IQueryable<Ticket> query = _context.Tickets;

            foreach (var item in conditions ?? new FilterConditions<Ticket>())
            {
                query = query.Where(item);
            }
            query = query.OrderByDescending(n => n.Id);

            await filter.Paging(query.Select(n => new ClientSideTicketDto()
            {
                Id = n.Id,
                Status = n.Status,
                Category = n.Category,
                CreatedDate = n.CreatedDate,
                LastUpdatedDate = n.LastUpdatedDate,
                Priority = n.Priority,
                Title = n.Title
            }));

            return filter;
        }

        public async Task<Result<ClientSideTicketDetailsDto>> ClientSideGetTicketDetails(int ticketId)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure<ClientSideTicketDetailsDto>(Messages.OperationFailedError);

            var ticket = await _context.Tickets
                .AsNoTracking()
                .Where(n => n.Id == ticketId && n.UserId == userId)
                .Select(n => new ClientSideTicketDetailsDto()
                {
                    Id = n.Id,
                    Category = n.Category,
                    CreatedDate = n.CreatedDate,
                    Status = n.Status,
                    Title = n.Title,
                    Messages = n.Messages
                    .OrderBy(m => m.Id)
                    .Select(m => new ClientSideTicketMessageDto()
                    {
                        Id = m.Id,
                        Text = m.Text,
                        CreatedDate = m.CreatedDate,
                        AttachmentPath = m.AttachmentPath,
                        IsAdminResponse = m.IsAdminResponse,
                    }).ToList()
                }).FirstOrDefaultAsync();
            if (ticket is null)
                return Result.Failure<ClientSideTicketDetailsDto>(Messages.OperationFailedError);

            return ticket;
        }

        public async Task<Result> ClientSideCloseTicket(int ticketId)
        {
            if (ticketId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var ticket = await _context.Tickets.Where(n => n.Id == ticketId && n.UserId == userId).FirstOrDefaultAsync();
            if (ticket is null)
                return Result.Failure(Messages.OperationFailedError);

            ticket.Status = TicketStatusEnum.Closed;
            ticket.LastUpdatedDate = DateTime.UtcNow;

            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();

            return Result.Success(Messages.TicketClosedSuccessfully);
        }

        public async Task<Result> ClientSideSendNewMessage(ClientSideSendTicketMessageDto dto)
        {
            if (dto is null)
                return Result.Failure(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext?.User.GetUserId() ?? 0;
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.Id == dto.TicketId && t.UserId == userId);

            if (ticket is null)
                return Result.Failure(Messages.OperationFailedError);

            var ticketMessage = new TicketMessage()
            {
                TicketId = dto.TicketId,
                SenderId = userId,
                Text = dto.MessageText,
                IsAdminResponse = false
            };

            string? fileName = null;
            if (dto.Attachment is not null)
            {
                if (!dto.Attachment.IsImage())
                    return Result.Failure(Messages.InvalidFileFormatError);

                if (dto.Attachment.Length > SiteTools.MaxTicketAttachmentLength.CalculateKB())
                    return Result.Failure<int>(Messages.MaxLengthError);

                fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(dto.Attachment.FileName);
                ticketMessage.AttachmentPath = fileName;
            }

            ticket.Status = TicketStatusEnum.Open;
            ticket.LastUpdatedDate = DateTime.UtcNow;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (dto.Attachment is not null && fileName is not null)
                {
                    dto.Attachment.UploadSensitiveFileToServer(fileName, SiteTools.TicketAttachmentFile);
                }

                _context.TicketMessages.Add(ticketMessage);
                _context.Tickets.Update(ticket);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Result.Success();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                if (dto.Attachment is not null && fileName is not null)
                {
                    fileName.RemoveSensitiveFileFromServer(SiteTools.TicketAttachmentFile);
                }

                return Result.Failure(Messages.OperationFailedError);
            }
        }
    }
}
