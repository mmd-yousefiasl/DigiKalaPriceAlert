using Application.Dtos.ContactUs.AdminSide;
using Application.Dtos.ContactUs.ClientSide;
using Application.Mediator;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Events.ContactUs;
using Domain.Models.ContactUs;
using Domain.Shared;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Implementaions
{
    public class ContactUsFormService : IContactUsFormService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediatorHandler _mediator;
        #endregion

        #region Ctor
        public ContactUsFormService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IMediatorHandler mediator)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }
        #endregion

        public async Task<Result> ClientSideSendNewContactUsForm(ClientSideSendNewContactUsFormDto dto)
        {
            if (dto is null)
                return Result.Failure(Messages.OperationFailedError);

            var contactUs = new ContactUsFrom()
            {
                FullName = dto.FullName,
                Subject = dto.Subject,
                Message = dto.Message,
                Mobile = dto.Mobile,
                UserIP = _httpContextAccessor.HttpContext.GetUserIP()
            };

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId > 0)
                contactUs.UserId = userId;

            _context.ContactUsForms.Add(contactUs);
            await _context.SaveChangesAsync();

            ContactUsFormCreatedEvent ContactUsFormCreatedEvent = new ContactUsFormCreatedEvent(
            contactUs.CreatedDate,
            contactUs.FullName,
            contactUs.Mobile,
            contactUs.Subject,
            contactUs.Message,
            contactUs.IsRead,
            contactUs.ReadByUserId,
            contactUs.UserId,
            contactUs.UserIP);

            await _mediator.PublishEvent(ContactUsFormCreatedEvent);

            return Result.Success(Messages.ContactUsFormSentSuccessfully);
        }

        public async Task<Result<AdminSideFilterContactUsFormsDto>> AdminSideFilterContactUsFomrs(AdminSideFilterContactUsFormsDto filter)
        {
            if (filter is null)
                return Result.Failure<AdminSideFilterContactUsFormsDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<ContactUsFrom>();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.FullName.Contains(filter.Search) ||
                n.Mobile.Contains(filter.Search) ||
                n.Message.Contains(filter.Search) ||
                n.UserIP.Contains(filter.Search));
            }

            if (filter.Subject is not null)
            {
                conditions.Add(n => n.Subject == filter.Subject);
            }

            if (filter.IsRead is not null)
            {
                conditions.Add(n => n.IsRead == filter.IsRead);
            }

            IQueryable<ContactUsFrom> query = _context.ContactUsForms;
            query = query.OrderByDescending(n => n.Id);

            foreach (var item in conditions ?? new FilterConditions<ContactUsFrom>())
            {
                query = query.Where(item);
            }

            await filter.Paging(query.Select(n => new AdminSideContactUsFormDto()
            {
                Id = n.Id,
                CreatedDate = n.CreatedDate,
                FullName = n.FullName,
                Message = n.Message,
                Mobile = n.Mobile,
                UserId = n.UserId,
                UserIP = n.UserIP,
                Subject = n.Subject,
                IsRead = n.IsRead
            }));

            return filter;
        }

        public async Task<Result> AdminSideDeleteContactUsForm(int id)
        {
            if (id < 1)
                return Result.Failure(Messages.OperationFailedError);

            var contactUsForm = await _context.ContactUsForms.FindAsync(id);
            if (contactUsForm is null)
                return Result.Failure(Messages.OperationFailedError);

            contactUsForm.IsDeleted = true;
            contactUsForm.LastUpdatedDate = DateTime.UtcNow;

            _context.ContactUsForms.Update(contactUsForm);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> AdminSideCheckContactUsFormAsRead(int id)
        {
            if (id < 1)
                return Result.Failure(Messages.OperationFailedError);

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId < 1)
                return Result.Failure(Messages.OperationFailedError);

            var contactUsForm = await _context.ContactUsForms.FindAsync(id);
            if (contactUsForm is null)
                return Result.Failure(Messages.OperationFailedError);

            contactUsForm.LastUpdatedDate = DateTime.UtcNow;
            contactUsForm.IsRead = true;
            contactUsForm.ReadByUserId = userId;


            _context.ContactUsForms.Update(contactUsForm);
            await _context.SaveChangesAsync();

            return Result.Success(Messages.ContactUsFormMarkedAsReadSuccessfully);
        }
    }
}