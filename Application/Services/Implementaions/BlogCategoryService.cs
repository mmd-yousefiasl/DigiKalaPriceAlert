using Application.Dtos.BlogCategory.AdminSide;
using Application.Dtos.BlogCategory.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Models.Blogs;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class BlogCategoryService : IBlogCategoryService
    {
        #region Fields
        private readonly DigiKalaPriceAlertDbContext _context;
        #endregion

        #region Ctor
        public BlogCategoryService(DigiKalaPriceAlertDbContext context)
        {
            _context = context;
        }
        #endregion

        public async Task<Result> AdminSideCreateNewBlogCategory(AdminSideCreateBlogCategoryDto model)
        {
            if (model is null)
                return Result.Failure(Messages.OperationFailedError);

            if (await _context.BlogCategories.AnyAsync(n => n.Slug == model.Slug.Replace(" ", "-")))
                return Result.Failure(Messages.DuplicateSlugError);

            BlogCategory category = new BlogCategory()
            {
                Title = model.Title,
                Slug = model.Slug.Replace(" ", "-")
            };

            _context.BlogCategories.Add(category);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> AdminSideDeleteCategory(int id)
        {
            if (id < 1)
                return Result.Failure(Messages.OperationFailedError);

            if (await _context.BlogCategories.AnyAsync(n => n.Id == id && n.Blogs.Any()))
                return Result.Failure(Messages.CantDeleteThisRecordError);

            var category = await _context.BlogCategories.FindAsync(id);
            if (category is null)
                return Result.Failure(Messages.OperationFailedError);

            _context.BlogCategories.Remove(category);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<AdminSideFilterBlogCategoriesDto>> AdminSideFilterCategories(AdminSideFilterBlogCategoriesDto filter)
        {
            if (filter is null) return Result.Failure<AdminSideFilterBlogCategoriesDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<BlogCategory>();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.Title.Contains(filter.Search));
            }

            IQueryable<BlogCategory> query = _context.BlogCategories;
            query = query.OrderByDescending(n => n.Id);

            foreach (var item in conditions ?? new FilterConditions<BlogCategory>())
            {
                query = query.Where(item);
            }

            await filter.Paging(query.Select(n => new AdminSideBlogCategoryDto()
            {
                Id = n.Id,
                Title = n.Title,
                Slug = n.Slug
            }));

            return filter;
        }

        public async Task<Result<List<AdminSideBlogCategoryDto>>> AdminSideGetCategories()
        {
            var categories = await _context.BlogCategories.Select(n => new AdminSideBlogCategoryDto()
            {
                Id = n.Id,
                Title = n.Title
            }).AsNoTracking().ToListAsync();
            if (!categories.Any())
                return Result.Failure<List<AdminSideBlogCategoryDto>>(Messages.OperationFailedError);

            return categories;
        }

        public async Task<Result<AdminSideUpdateBlogCategoryDto>> AdminSideGetCategoryForUpdate(int id)
        {
            if (id < 1) return Result.Failure<AdminSideUpdateBlogCategoryDto>(Messages.OperationFailedError);

            var category = await _context.BlogCategories.Where(n => n.Id == id).Select(n => new AdminSideUpdateBlogCategoryDto()
            {
                Id = n.Id,
                Title = n.Title,
                Slug = n.Slug
            }).AsNoTracking().FirstOrDefaultAsync();
            if (category is null)
                return Result.Failure<AdminSideUpdateBlogCategoryDto>(Messages.OperationFailedError);

            return category;
        }

        public async Task<Result> AdminSideUpdateCategory(AdminSideUpdateBlogCategoryDto model)
        {
            if (model is null)
                return Result.Failure(Messages.OperationFailedError);

            if (await _context.BlogCategories.AnyAsync(n => n.Id != model.Id && n.Slug == model.Slug.Replace(" ", "-")))
                return Result.Failure(Messages.DuplicateSlugError);

            var category = await _context.BlogCategories.FindAsync(model.Id);
            if (category is null)
                return Result.Failure(Messages.OperationFailedError);

            category.Title = model.Title;
            category.Slug = model.Slug.Replace(" ", "-");

            _context.BlogCategories.Update(category);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<List<ClientSideBlogCategoryDto>>> ClientSideGetCategories()
        {
            var categories = await _context.BlogCategories.Select(n => new ClientSideBlogCategoryDto()
            {
                Id = n.Id,
                Title = n.Title,
                Slug = n.Slug
            }).AsNoTracking().ToListAsync();

            if (!categories?.Any() ?? true)
                return Result.Failure<List<ClientSideBlogCategoryDto>>(Messages.OperationFailedError);

            return categories;
        }
    }
}