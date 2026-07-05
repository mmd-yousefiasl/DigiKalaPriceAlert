using Application.Dtos.Media.AdminSide;
using Application.Dtos.Media.ClientSide;
using Application.Mediator;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Events.SocialMedia;
using Domain.Models.Media;
using Domain.Shared;
using Domain.Statics;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class SocialMediaService : ISocialMediaService
    {
        #region Fields
        private readonly DigiKalaPriceAlertDbContext _context;
        private readonly IMediatorHandler _mediator;
        private readonly ICacheManager _cacheManager;
        #endregion

        #region Ctor
        public SocialMediaService(DigiKalaPriceAlertDbContext context,
            IMediatorHandler mediator,
            ICacheManager cacheManager)
        {
            _context = context;
            _mediator = mediator;
            _cacheManager = cacheManager;
        }
        #endregion

        public async Task<Result> AdminSideCreateNewSocialMedia(AdminSideCreateNewSocialMediaDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);
            if (!model.File.IsImage()) return Result.Failure(Messages.InvalidFileFormatError);
            if (model.File.Length > 400.CalculateKB()) return Result.Failure(Messages.FileLengthError);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
            model.File.UploadFileToServer(fileName, SiteTools.SocialMediaFile);

            SocialMedia media = new SocialMedia()
            {
                FileName = fileName,
                Title = model.Title,
                Url = model.Url,
                IsPublished = model.IsPublished,
                Priority = model.Priority,
            };

            _context.SocialMedia.Add(media);
            await _context.SaveChangesAsync();

            SocialMediaCreatedEvent SocialMediaCreatedEvent = new SocialMediaCreatedEvent(
            media.FileName,
            media.Title,
            media.Url,
            media.Priority,
            media.IsPublished);

            await _mediator.PublishEvent(SocialMediaCreatedEvent);

            return Result.Success();
        }

        public async Task<Result> AdminSideDeleteSocialMedia(int id)
        {
            if (id < 1) return Result.Failure(Messages.OperationFailedError);

            var media = await _context.SocialMedia.FindAsync(id);
            if (media is null) return Result.Failure(Messages.OperationFailedError);

            media.FileName.RemoveFileFromServer(SiteTools.SocialMediaFile);

            _context.SocialMedia.Remove(media);
            await _context.SaveChangesAsync();

            SocialMediaDeletedEvent SocialMediaDeletedEvent = new SocialMediaDeletedEvent(media.Id);

            await _mediator.PublishEvent(SocialMediaDeletedEvent);

            return Result.Success();
        }

        public async Task<Result<AdminSideFilterSocialMediaDto>> AdminSideFilterSocialMedia(AdminSideFilterSocialMediaDto filter)
        {
            if (filter is null)
                return Result.Failure<AdminSideFilterSocialMediaDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<SocialMedia>();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.Title.Contains(filter.Search));
            }

            if (filter.IsPublished is not null)
            {
                conditions.Add(n => n.IsPublished == filter.IsPublished);
            }

            IQueryable<SocialMedia> query = _context.SocialMedia;
            query = query.OrderByDescending(n => n.Id);

            foreach (var item in conditions ?? new FilterConditions<SocialMedia>())
            {
                query = query.Where(item);
            }

            await filter.Paging(query.Select(n => new AdminSideSocialMediaDto()
            {
                Id = n.Id,
                Title = n.Title,
                IsPublished = n.IsPublished,
                Priority = n.Priority
            }));

            return filter;
        }

        public async Task<Result<AdminSideUpdateSocialMediaDto>> AdminSideGetSocialMediaForUpdate(int id)
        {
            if (id < 1) return Result.Failure<AdminSideUpdateSocialMediaDto>(Messages.OperationFailedError);

            var media = await _context.SocialMedia.Where(n => n.Id == id).Select(n => new AdminSideUpdateSocialMediaDto()
            {
                Id = n.Id,
                FileName = n.FileName,
                IsPublished = n.IsPublished,
                Priority = n.Priority,
                Title = n.Title,
                Url = n.Url,
            }).AsNoTracking().FirstOrDefaultAsync();
            if (media is null)
                return Result.Failure<AdminSideUpdateSocialMediaDto>(Messages.OperationFailedError);

            return media;
        }

        public async Task<Result> AdminSideUpdateSocialMedia(AdminSideUpdateSocialMediaDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);

            var media = await _context.SocialMedia.FindAsync(model.Id);
            if (media is null) return Result.Failure(Messages.OperationFailedError);

            media.Title = model.Title;
            media.Url = model.Url;
            media.Priority = model.Priority;
            media.IsPublished = model.IsPublished;

            if (model.File is not null)
            {
                if (!model.File.IsImage()) return Result.Failure(Messages.InvalidFileFormatError);
                if (model.File.Length > 400.CalculateKB()) return Result.Failure(Messages.FileLengthError);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
                model.File.UploadFileToServer(fileName, SiteTools.SocialMediaFile);
                media.FileName.RemoveFileFromServer(SiteTools.SocialMediaFile);

                media.FileName = fileName;
            }

            _context.SocialMedia.Update(media);
            await _context.SaveChangesAsync();

            SocialMediaUpdatedEvent SocialMediaUpdatedEvent = new SocialMediaUpdatedEvent(
            media.Id,
            media.FileName,
            media.Title,
            media.Url,
            media.Priority,
            media.IsPublished);

            await _mediator.PublishEvent(SocialMediaUpdatedEvent);

            return Result.Success();
        }

        public async Task<Result<List<ClientSideSocialMediaDto>>> ClientSideGetSocialMedia()
        {
            var media = await _cacheManager.GetAsync(CacheKeys.SocialMediaCacheKey, async () =>
            {
                return await _context.SocialMedia.Where(n => n.IsPublished).OrderBy(n => n.Priority)
                .Select(n => new ClientSideSocialMediaDto()
                {
                    FileName = n.FileName,
                    Title = n.Title,
                    Url = n.Url
                }).AsNoTracking().ToListAsync();
            });

            if (!media?.Any() ?? true)
                return Result.Failure<List<ClientSideSocialMediaDto>>(Messages.OperationFailedError);

            return media;
        }
    }
}
