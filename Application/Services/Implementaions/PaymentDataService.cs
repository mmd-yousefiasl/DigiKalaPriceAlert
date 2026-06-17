using Application.Dtos.PaymentData;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Shared;
using Domain.Statics;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Application.Services.Implementaions
{
    public class PaymentDataService : IPaymentDataService
    {
        #region Fields
        private readonly DigiKalaPriceAlertDbContext _context;
        #endregion

        #region Ctor
        public PaymentDataService(DigiKalaPriceAlertDbContext context)
        {
            _context = context;
        }
        #endregion

        public async Task<Result<AdminSideUpdatePaymentDataDto>> AdminSideGetPaymentDataForUpdate()
        {
            var data = await _context.PaymentData.Where(n => n.Id == 1).Select(n => new AdminSideUpdatePaymentDataDto()
            {
                PaymentGatewayPin = n.PaymentGatewayPin,
                Url = n.Url,
                FileName = n.FileName,
            }).AsNoTracking().FirstOrDefaultAsync();
            if (data is null)
                return Result.Failure<AdminSideUpdatePaymentDataDto>(Messages.OperationFailedError);

            return data;
        }

        public async Task<Result> AdminSideUpdatePaymentData(AdminSideUpdatePaymentDataDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);

            var data = await _context.PaymentData.FindAsync(1);
            if (data is null) return Result.Failure(Messages.OperationFailedError);

            data.PaymentGatewayPin = model.PaymentGatewayPin;
            data.Url = model.Url;

            if (model.File is not null)
            {
                if (!model.File.IsImage()) return Result.Failure(Messages.InvalidFileFormatError);
                if (model.File.Length > 400.CalculateKB()) return Result.Failure(Messages.FileLengthError);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
                model.File.UploadFileToServer(fileName, SiteTools.PaymentLogo);
                data.FileName?.RemoveFileFromServer(SiteTools.PaymentLogo);

                data.FileName = fileName;
            }

            _context.PaymentData.Update(data);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<ClientSidePaymentDataLogoDto>> ClientSideGetPaymentDataLogo()
        {
            var data = await _context.PaymentData.Where(n => n.Id == 1).AsNoTracking().FirstOrDefaultAsync();
            if (data is null)
                return Result.Failure<ClientSidePaymentDataLogoDto>(Messages.OperationFailedError);

            return new ClientSidePaymentDataLogoDto()
            {
                FileName = data.FileName,
                Url = data.Url
            };
        }

        public async Task<Result<string>> GetPin()
        {
            var data = await _context.PaymentData.Where(n => n.Id == 1).AsNoTracking().FirstOrDefaultAsync();
            if (data is null)
                return Result.Failure<string>(Messages.OperationFailedError);

            if (string.IsNullOrWhiteSpace(data.PaymentGatewayPin)) return Result.Failure<string>(Messages.OperationFailedError);

            return data.PaymentGatewayPin;
        }
    }
}