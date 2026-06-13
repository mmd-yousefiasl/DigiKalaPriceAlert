using Application.Dtos.EmailData.AdminSide;
using Application.Services.Interfaces;
using Data.Context;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class EmailDataService : IEmailDataService
    {
        #region Fields
        private readonly DigiKalaPriceAlertDbContext _context;
        #endregion

        #region Ctor
        public EmailDataService(DigiKalaPriceAlertDbContext context)
        {
            _context = context;
        }
        #endregion

        public async Task<Result<AdminSideEmailDataDto>> AdminSideGetEmailData()
        {
            var data = await _context.EmailData.Where(n => n.Id == 1).Select(n => new AdminSideEmailDataDto()
            {
                Id = n.Id,
                Smtp = n.Smtp,
                EmailDisplayName = n.EmailDisplayName,
                From = n.From,
                Password = n.Password,
                Port = n.Port ?? 1
            }).AsNoTracking().FirstOrDefaultAsync();

            if (data is null)
                return Result.Failure<AdminSideEmailDataDto>(Messages.OperationFailedError);

            return data;
        }

        public async Task<Result<AdminSideUpdateEmailDataDto>> AdminSideGetEmailDataForUpdate()
        {
            var emailData = await _context.EmailData.Where(n => n.Id == 1).Select(n => new AdminSideUpdateEmailDataDto()
            {
                Id = n.Id,
                Smtp = n.Smtp,
                EmailDisplayName = n.EmailDisplayName,
                From = n.From,
                Password = n.Password,
                Port = n.Port ?? 1,
            }).AsNoTracking().FirstOrDefaultAsync();

            if (emailData is null)
                return Result.Failure<AdminSideUpdateEmailDataDto>(Messages.OperationFailedError);

            return emailData;
        }

        public async Task<Result> AdminSideUpdateEmailData(AdminSideUpdateEmailDataDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);

            var emailData = await _context.EmailData.FindAsync(1);
            if (emailData is null) return Result.Failure(Messages.OperationFailedError);

            emailData.From = model.From;
            emailData.Password = model.Password;
            emailData.EmailDisplayName = model.EmailDisplayName;
            emailData.Smtp = model.Smtp;
            emailData.Port = model.Port;

            _context.EmailData.Update(emailData);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
    }
}
