using Application.Dtos.Medium.AdminSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Models.Media;
using Domain.Shared;
using Domain.Statics;

namespace Application.Services.Implementaions
{
    public class MediumService : IMediumService
    {
        #region Fields
        private readonly DigiKalaPriceAlertDbContext _context;
        #endregion

        #region Ctor
        public MediumService(DigiKalaPriceAlertDbContext context)
        {
            _context = context;
        }
        #endregion

        public async Task<Result> AdminSideCreateNewMedia(AdminSideCreateNewMediumDto dto)
        {
            if (dto is null)
                return Result.Failure(Messages.OperationFailedError);

            if (!dto.File.IsImage())
                return Result.Failure(Messages.InvalidFileFormatError);

            if (dto.File.Length > SiteTools.MaxMediumFileLength.CalculateKB())
                return Result.Failure(Messages.FileLengthError);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.File.FileName);
            dto.File.UploadFileToServer(fileName, SiteTools.MediumFile);

            var media = new Medium()
            {
                FileName = fileName,
            };

            _context.Media.Add(media);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> AdminSideDeleteMedia(int id)
        {
            if (id < 1) return Result.Failure(Messages.OperationFailedError);

            var media = await _context.Media.FindAsync(id);
            if (media is null) return Result.Failure(Messages.OperationFailedError);

            media.FileName.RemoveFileFromServer(SiteTools.MediumFile);

            _context.Media.Remove(media);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<AdminSideFilterMediaDto>> AdminSideFilterMedia(AdminSideFilterMediaDto filter)
        {
            if (filter is null)
                return Result.Failure<AdminSideFilterMediaDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<Medium>();

            IQueryable<Medium> query = _context.Media;
            query = query.OrderByDescending(n => n.Id);

            await filter.Paging(query.Select(n => new AdminSideMediumDto()
            {
                Id = n.Id,
                FileName = n.FileName
            }));

            return filter;
        }
    }
}
