using Application.Dtos.ContactUs.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
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
        #endregion

        #region Ctor
        public ContactUsFormService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        public async Task<Result> ClientSideSendNewContactUsForm(ClientSideSendNewContactUsFormDto dto)
        {
            if (dto is null)
                return Result.Failure(Messages.OperationFailedError);

            var contactUs = new ContactUsFrom()
            {
                Name = dto.Name,
                Subject = dto.Subject,
                Message = dto.Message,
                MobileOrId = dto.MobileOrId,
                UserIP = _httpContextAccessor.HttpContext.GetUserIP()
            };

            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            if (userId > 0)
                contactUs.UserId = userId;

            _context.ContactUsFroms.Add(contactUs);
            await _context.SaveChangesAsync();

            return Result.Success(Messages.ContactUsFormSentSuccessfully);
        }
    }
}
