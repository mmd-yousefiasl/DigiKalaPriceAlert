using Application.Dtos.SiteMap.AdminSide;
using Application.Dtos.SiteMap.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Enums.GeneralInfo;
using Domain.Models.GeneralInfo;
using Domain.Shared;
using Domain.Statics;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class SiteMapService : ISiteMapService
    {
        #region Fields
        private readonly DigiKalaPriceAlertDbContext _context;
        private readonly IBlogCategoryService _blogCategoryService;
        private readonly IBlogService _blogService;
        #endregion

        #region Ctor
        public SiteMapService(DigiKalaPriceAlertDbContext context,
            IBlogCategoryService blogCategoryService,
            IBlogService blogService)
        {
            _context = context;
            _blogCategoryService = blogCategoryService;
            _blogService = blogService;
        }
        #endregion

        public async Task<Result> AdminSideCreateNewSiteMap(AdminSideCreateNewSiteMapDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);

            var map = new SiteMap()
            {
                Url = model.Url,
                ChangeFreq = model.ChangeFreq,
                Priority = model.Priority,
                IsPublished = model.IsPublished,
                Type = model.Type
            };

            _context.SiteMap.Add(map);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> AdminSideDeleteSiteMap(int id)
        {
            if (id < 1) return Result.Failure(Messages.OperationFailedError);

            var map = await _context.SiteMap.FindAsync(id);
            if (map is null) return Result.Failure(Messages.OperationFailedError);

            _context.SiteMap.Remove(map);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<AdminSideFilterSiteMapDto>> AdminSideFilterSiteMap(AdminSideFilterSiteMapDto filter)
        {
            if (filter is null) return Result.Failure<AdminSideFilterSiteMapDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<SiteMap>();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.Url.Contains(filter.Search));
            }


            if (filter.ChangeFreq is not null)
            {
                conditions.Add(n => n.ChangeFreq == filter.ChangeFreq);
            }

            if (filter.IsPublished is not null)
            {
                conditions.Add(n => n.IsPublished == filter.IsPublished);
            }

            if (filter.Type is not null)
            {
                conditions.Add(n => n.Type == filter.Type);
            }

            IQueryable<SiteMap> query = _context.SiteMap;
            query = query.OrderByDescending(n => n.Id);

            foreach (var item in conditions ?? new FilterConditions<SiteMap>())
            {
                query = query.Where(item);
            }

            await filter.Paging(query.Select(n => new AdminSideSiteMapDto()
            {
                Id = n.Id,
                ChangeFreq = n.ChangeFreq,
                IsPublished = n.IsPublished,
                Priority = n.Priority,
                Type = n.Type,
                Url = n.Url
            }));

            return filter;
        }

        public async Task<Result<AdminSideUpdateSiteMapDto>> AdminSideGetSiteMapForUpdate(int id)
        {
            if (id < 1) return Result.Failure<AdminSideUpdateSiteMapDto>(Messages.OperationFailedError);

            var map = await _context.SiteMap.Where(n => n.Id == id).Select(n => new AdminSideUpdateSiteMapDto()
            {
                Id = n.Id,
                Url = n.Url,
                ChangeFreq = n.ChangeFreq,
                IsPublished = n.IsPublished,
                Priority = n.Priority,
                Type = n.Type,
            }).AsNoTracking().FirstOrDefaultAsync();
            if (map is null)
                return Result.Failure<AdminSideUpdateSiteMapDto>(Messages.OperationFailedError);

            return map;
        }

        public async Task<Result> AdminSideUpdateSiteMap(AdminSideUpdateSiteMapDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);

            var map = await _context.SiteMap.FindAsync(model.Id);
            if (map is null) return Result.Failure(Messages.OperationFailedError);

            map.Url = model.Url;
            map.ChangeFreq = model.ChangeFreq;
            map.Priority = model.Priority;
            map.IsPublished = model.IsPublished;
            map.Type = model.Type;
            map.LastUpdatedDate = DateTime.Now;

            _context.SiteMap.Update(map);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> AdminSideDeleteAllLinks()
        {
            var maps = await _context.SiteMap.ToListAsync();
            if (!maps.Any()) return Result.Failure(Messages.OperationFailedError);

            _context.SiteMap.RemoveRange(maps);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> AdminSideDeleteAllExceptOtherPages()
        {
            var maps = await _context.SiteMap.Where(n => n.Type != SiteMapTypeEnum.OtherPages).ToListAsync();
            if (!maps.Any()) return Result.Failure(Messages.OperationFailedError);

            _context.SiteMap.RemoveRange(maps);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> UpdateBlogCategoriesSiteMap()
        {
            var blogCategoriesResult = await _blogCategoryService.GetCategoriesAsOptions();
            if (blogCategoriesResult.IsFailure)
                return Result.Failure(Messages.OperationFailedError);

            var categories = blogCategoriesResult.Value;

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Efficient Bulk Delete directly on Postgres (No in-memory loading)
                    await _context.SiteMap
                        .Where(n => n.Type == SiteMapTypeEnum.BlogCategory)
                        .ExecuteDeleteAsync();

                    var newMaps = categories.Select(n => new SiteMap
                    {
                        ChangeFreq = ChangeFreqEnum.Weekly,
                        Url = $"{SiteTools.SiteUrl}blogs/{n.Slug}",
                        IsPublished = true,
                        Type = SiteMapTypeEnum.BlogCategory,
                        Priority = 0.7m
                    }).ToList();

                    _context.SiteMap.AddRange(newMaps);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return Result.Success();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure(Messages.OperationFailedError);
                }
            });
        }

        public async Task<Result> UpdateBlogsSiteMap()
        {
            var blogResult = await _blogService.AdminSideGetBlogIdsForSiteMap();
            if (blogResult.IsFailure)
                return Result.Failure(Messages.OperationFailedError);

            var Ids = blogResult.Value;

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Efficient Bulk Delete directly on Postgres
                    await _context.SiteMap
                        .Where(n => n.Type == SiteMapTypeEnum.Blog)
                        .ExecuteDeleteAsync();

                    var newMaps = Ids.Select(n => new SiteMap
                    {
                        ChangeFreq = ChangeFreqEnum.Monthly,
                        Url = $"{SiteTools.SiteUrl}b/{n.Id}/{n.Slug}",
                        IsPublished = true,
                        Type = SiteMapTypeEnum.Blog,
                        Priority = 0.9m
                    }).ToList();

                    _context.SiteMap.AddRange(newMaps);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return Result.Success();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure(Messages.OperationFailedError);
                }
            });
        }

        public async Task<Result> UpdateAllSiteMap()
        {
            var bCategoryResult = await UpdateBlogCategoriesSiteMap();
            if (bCategoryResult.IsFailure) return Result.Failure(Messages.UpdateBlogCategoriesSiteMapError);

            var blogResult = await UpdateBlogsSiteMap();
            if (blogResult.IsFailure) return Result.Failure(Messages.UpdateBlogSiteMapError);

            return Result.Success();
        }

        public async Task<Result<List<ClientSideSiteMapDto>>> ClientSideGetSiteMap()
        {
            var maps = await _context.SiteMap.Where(n => n.IsPublished).OrderByDescending(n => n.Priority)
                .Select(n => new ClientSideSiteMapDto()
                {
                    Url = n.Url,
                    LastUpdateDate = n.LastUpdatedDate,
                    ChangeFreq = n.ChangeFreq,
                    Priority = n.Priority
                }).AsNoTracking().ToListAsync();

            if (!maps?.Any() ?? true)
                return Result.Failure<List<ClientSideSiteMapDto>>(Messages.OperationFailedError);

            return maps;
        }
    }
}