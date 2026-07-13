using Application.Dtos.Blog.AdminSide;
using Application.Dtos.Blog.ClientSide;
using Application.Dtos.BlogCategory.AdminSide;
using Application.Dtos.BlogCategory.ClientSide;
using Application.Mediator;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Events.BlogCategories;
using Domain.Events.Blogs;
using Domain.Models.Blogs;
using Domain.Shared;
using Domain.Statics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class BlogService : IBlogService
    {
        #region Fields
        private readonly DigiKalaPriceAlertDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediatorHandler _mediator;
        private readonly ICacheManager _cacheManager;
        #endregion

        #region Ctor
        public BlogService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor,
            IMediatorHandler mediator,
            ICacheManager cacheManager)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
            _cacheManager = cacheManager;
        }
        #endregion

        public async Task<Result> AdminSideCreateNewBlog(AdminSideCreateNewBlogDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);
            string slug = model.Slug.Replace(" ", "-");

            if (await _context.Blogs.AnyAsync(n => n.Slug == slug))
                return Result.Failure(Messages.DuplicateSlugError);

            if (!model.File.IsImage()) return Result.Failure(Messages.InvalidFileFormatError);
            if (model.File.Length > 400.CalculateKB()) return Result.Failure(Messages.FileLengthError);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
            model.File.UploadFileToServer(fileName, SiteTools.BlogFile);

            Blog blog = new Blog()
            {
                Title = model.Title,
                FileName = fileName,
                Description = model.Description,
                Priority = model.Priority,
                IsPublished = model.IsPublished,
                CategoryId = model.CategoryId,
                StudyTime = model.StudyTime,
                Writer = model.Writer,
                Slug = slug,
                MetaDescription = model.MetaDescription,
            };

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            BlogCreatedEvent BlogCreatedEvent = new BlogCreatedEvent(
            blog.FileName,
            blog.Title,
            blog.Slug,
            blog.Description,
            blog.Writer,
            blog.StudyTime,
            blog.Priority,
            blog.IsPublished,
            blog.CategoryId);

            await _mediator.PublishEvent(BlogCreatedEvent);

            return Result.Success();
        }

        public async Task<Result> AdminSideDeleteBlog(int id)
        {
            if (id < 1) return Result.Failure(Messages.OperationFailedError);

            var blog = await _context.Blogs.FindAsync(id);
            if (blog is null) return Result.Failure(Messages.OperationFailedError);

            blog.FileName.RemoveFileFromServer(SiteTools.BlogFile);

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            BlogDeletedEvent BlogDeletedEvent = new BlogDeletedEvent(blog.Id);

            await _mediator.PublishEvent(BlogDeletedEvent);

            return Result.Success();
        }

        public async Task<Result<AdminSideFIlterBlogsDto>> AdminSideFilterBlogs(AdminSideFIlterBlogsDto filter)
        {
            if (filter is null) return Result.Failure<AdminSideFIlterBlogsDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<Blog>();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.Title.Contains(filter.Search) ||
                n.Writer.Contains(filter.Search) ||
                n.Category.Title.Contains(filter.Search));
            }

            if (filter.IsPublished is not null)
            {
                conditions.Add(n => n.IsPublished == filter.IsPublished);
            }

            IQueryable<Blog> query = _context.Blogs;
            query = query.OrderByDescending(n => n.Id);

            foreach (var item in conditions ?? new FilterConditions<Blog>())
            {
                query = query.Where(item);
            }

            await filter.Paging(query.Select(n => new AdminSideBlogDto()
            {
                Id = n.Id,
                Title = n.Title,
                CategoryTitle = n.Category.Title,
                IsPublished = n.IsPublished,
                Priority = n.Priority,
                StudyTime = n.StudyTime,
                Writer = n.Writer,
                ViewsCount = n.Views.Count()
            }));

            return filter;
        }

        public async Task<Result<AdminSideUpdateBlogDto>> AdminSideGetBlogForUpdate(int id)
        {
            if (id < 1)
                return Result.Failure<AdminSideUpdateBlogDto>(Messages.OperationFailedError);

            var blog = await _context.Blogs
                .AsNoTracking()
                .Where(n => n.Id == id)
                .Select(n => new AdminSideUpdateBlogDto()
                {
                    Id = n.Id,
                    FileName = n.FileName,
                    Title = n.Title,
                    Description = n.Description,
                    IsPublished = n.IsPublished,
                    StudyTime = n.StudyTime,
                    CategoryId = n.CategoryId,
                    Priority = n.Priority,
                    Writer = n.Writer,
                    Slug = n.Slug,
                    MetaDescription = n.MetaDescription,
                }).FirstOrDefaultAsync();
            if (blog is null)
                return Result.Failure<AdminSideUpdateBlogDto>(Messages.OperationFailedError);

            return blog;
        }

        public async Task<Result> AdminSideUpdateBlog(AdminSideUpdateBlogDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);
            string slug = model.Slug.Replace(" ", "-");
            if (await _context.Blogs.AnyAsync(n => n.Id != model.Id && n.Slug == model.Slug))
                return Result.Failure(Messages.DuplicateSlugError);

            var blog = await _context.Blogs.FindAsync(model.Id);
            if (blog is null) return Result.Failure(Messages.OperationFailedError);

            blog.Title = model.Title;
            blog.Description = model.Description;
            blog.Writer = model.Writer;
            blog.StudyTime = model.StudyTime;
            blog.Priority = model.Priority;
            blog.IsPublished = model.IsPublished;
            blog.CategoryId = model.CategoryId;
            blog.Slug = slug;
            blog.MetaDescription = model.MetaDescription;
            blog.LastUpdatedDate = DateTime.UtcNow;

            if (model.File is not null)
            {
                if (!model.File.IsImage()) return Result.Failure(Messages.InvalidFileFormatError);
                if (model.File.Length > 400.CalculateKB()) return Result.Failure(Messages.FileLengthError);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);
                model.File.UploadFileToServer(fileName, SiteTools.BlogFile);
                blog.FileName.RemoveFileFromServer(SiteTools.BlogFile);

                blog.FileName = fileName;
            }

            _context.Blogs.Update(blog);
            await _context.SaveChangesAsync();

            BlogUpdatedEvent BlogUpdatedEvent = new BlogUpdatedEvent(
            blog.Id,
            blog.FileName,
            blog.Title,
            blog.Slug,
            blog.Description,
            blog.Writer,
            blog.StudyTime,
            blog.Priority,
            blog.IsPublished,
            blog.CategoryId);

            await _mediator.PublishEvent(BlogUpdatedEvent);

            return Result.Success();
        }

        public async Task<Result<ClientSideFilterBlogsDto>> ClientSideFilterBlogs(ClientSideFilterBlogsDto filter)
        {
            if (filter is null) return Result.Failure<ClientSideFilterBlogsDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<Blog>();

            conditions.Add(n => n.IsPublished == true);

            if (!string.IsNullOrWhiteSpace(filter.CategorySlug))
            {
                var categoryResult = await ClientSideGetCategoryBySlug(filter.CategorySlug);
                if (categoryResult.IsSuccess)
                {
                    filter.CategoryMetaDescription = categoryResult.Value.MetaDescription;
                    filter.CategoryTitle = categoryResult.Value.Title;
                }

                conditions.Add(n => n.Category.Slug == filter.CategorySlug);
            }

            IQueryable<Blog> query = _context.Blogs;
            query = query.OrderByDescending(n => n.Id);

            foreach (var item in conditions ?? new FilterConditions<Blog>())
            {
                query = query.Where(item);
            }

            await filter.Paging(query.Select(n => new ClientSideBlogDto()
            {
                Id = n.Id,
                Slug = n.Slug,
                Title = n.Title,
                CreatedDate = n.CreatedDate,
                FileName = n.FileName,
            }));

            return filter;
        }

        public async Task<Result<ClientSideBlogDetailsDto>> ClientSideGetBlogDetails(int id)
        {
            if (id < 1) return Result.Failure<ClientSideBlogDetailsDto>(Messages.OperationFailedError);

            var blog = await _context.Blogs
                .AsNoTracking()
                .Where(n => n.Id == id && n.IsPublished)
                .Select(n => new ClientSideBlogDetailsDto()
                {
                    Id = n.Id,
                    Title = n.Title,
                    Slug = n.Slug,
                    Description = n.Description,
                    Category = n.Category.Title,
                    CategorySlug = n.Category.Slug,
                    CategoryId = n.CategoryId,
                    CreatedDate = n.CreatedDate,
                    StudyTime = n.StudyTime,
                    FileName = n.FileName,
                    Writer = n.Writer,
                    MetaDescription = n.MetaDescription,
                    ViewsCount = n.Views.Count()
                }).FirstOrDefaultAsync();
            if (blog is null)
                return Result.Failure<ClientSideBlogDetailsDto>(Messages.OperationFailedError);

            return blog;
        }

        public async Task<Result<List<ClientSideBlogDto>>> ClientSideGetHighPriorityBlogs(int take)
        {
            if (take < 1) return Result.Failure<List<ClientSideBlogDto>>(Messages.OperationFailedError);

            var blogs = await _cacheManager.GetAsync(CacheKeys.BlogCacheKey, async () =>
            {
                return await _context.Blogs
                .AsNoTracking()
                .Where(n => n.IsPublished)
                .OrderByDescending(n => n.Id)
                .Take(take)
                .Select(n => new ClientSideBlogDto()
                {
                    Id = n.Id,
                    Title = n.Title,
                    Slug = n.Slug,
                    CreatedDate = n.CreatedDate,
                    FileName = n.FileName
                }).ToListAsync();
            });

            if (!blogs?.Any() ?? true)
                return Result.Failure<List<ClientSideBlogDto>>(Messages.OperationFailedError);

            return blogs;
        }

        public async Task<Result<List<ClientSideBlogDto>>> ClientSideGetRelatedBlogs(int blogId, int categoryId)
        {
            var blogs = await _context.Blogs
                .AsNoTracking()
                .Where(n => n.IsPublished && n.Id != blogId && n.CategoryId == categoryId)
                .OrderBy(n => n.Priority)
                .Take(5)
                .Select(n => new ClientSideBlogDto()
                {
                    Id = n.Id,
                    CreatedDate = n.CreatedDate,
                    Slug = n.Slug,
                    FileName = n.FileName,
                    Title = n.Title
                }).ToListAsync();
            if (!blogs.Any())
                return Result.Failure<List<ClientSideBlogDto>>(Messages.OperationFailedError);

            return blogs;
        }

        public async Task<Result<List<AdminSideBlogForSiteMapDto>>> AdminSideGetBlogIdsForSiteMap()
        {
            var ids = await _context.Blogs.Where(n => n.IsPublished)
                .Select(n => new AdminSideBlogForSiteMapDto()
                {
                    Id = n.Id,
                    Slug = n.Slug
                })
                .AsNoTracking()
                .ToListAsync();
            if (!ids.Any())
                return Result.Failure<List<AdminSideBlogForSiteMapDto>>(Messages.OperationFailedError);

            return ids;
        }

        public async Task<Result> AdminSideCreateNewBlogCategory(AdminSideCreateBlogCategoryDto model)
        {
            if (model is null)
                return Result.Failure(Messages.OperationFailedError);

            if (await _context.BlogCategories.AnyAsync(n => n.Slug == model.Slug.Replace(" ", "-")))
                return Result.Failure(Messages.DuplicateSlugError);

            BlogCategory category = new BlogCategory()
            {
                Title = model.Title,
                Slug = model.Slug.Replace(" ", "-"),
                IsPublished = model.IsPublished,
                Priority = model.Priority,
                MetaDescription = model.MetaDescription,
            };

            _context.BlogCategories.Add(category);
            await _context.SaveChangesAsync();

            BlogCategoryCreatedEvent BlogCategoryCreatedEvent = new BlogCategoryCreatedEvent(
            category.Title,
            category.Slug,
            category.MetaDescription,
            category.Priority,
            category.IsPublished);

            await _mediator.PublishEvent(BlogCategoryCreatedEvent);

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

            BlogCategoryDeletedEvent BlogCategoryDeletedEvent = new BlogCategoryDeletedEvent(category.Id);

            await _mediator.PublishEvent(BlogCategoryDeletedEvent);

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
                Slug = n.Slug,
                Priority = n.Priority,
                IsPublished = n.IsPublished
            }));

            return filter;
        }

        public async Task<Result<AdminSideUpdateBlogCategoryDto>> AdminSideGetCategoryForUpdate(int id)
        {
            if (id < 1) return Result.Failure<AdminSideUpdateBlogCategoryDto>(Messages.OperationFailedError);

            var category = await _context.BlogCategories.Where(n => n.Id == id).Select(n => new AdminSideUpdateBlogCategoryDto()
            {
                Id = n.Id,
                Title = n.Title,
                Slug = n.Slug,
                Priority = n.Priority,
                IsPublished = n.IsPublished,
                MetaDescription = n.MetaDescription,
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
            category.IsPublished = model.IsPublished;
            category.Priority = model.Priority;
            category.MetaDescription = model.MetaDescription;

            _context.BlogCategories.Update(category);
            await _context.SaveChangesAsync();

            BlogCategoryUpdatedEvent BlogCategoryUpdatedEvent = new BlogCategoryUpdatedEvent(
            category.Id,
            category.Title,
            category.Slug,
            category.MetaDescription,
            category.Priority,
            category.IsPublished);

            await _mediator.PublishEvent(BlogCategoryUpdatedEvent);

            return Result.Success();
        }

        public async Task<Result<List<ClientSideBlogCategoryDto>>> ClientSideGetCategories()
        {
            var categories = await _cacheManager.GetAsync(CacheKeys.BlogCategoryCacheKey, async () =>
            {
                return await _context.BlogCategories
                .AsNoTracking()
                .Where(n => n.IsPublished)
                .OrderBy(n => n.Priority)
                .Select(n => new ClientSideBlogCategoryDto()
                {
                    Id = n.Id,
                    Title = n.Title,
                    Slug = n.Slug,
                    MetaDescription = n.MetaDescription
                }).ToListAsync();
            });

            if (!categories?.Any() ?? true)
                return Result.Failure<List<ClientSideBlogCategoryDto>>(Messages.OperationFailedError);

            return categories;
        }

        public async Task<Result<ClientSideBlogCategoryDto>> ClientSideGetCategoryBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return Result.Failure<ClientSideBlogCategoryDto>(Messages.OperationFailedError);

            var categories = await _cacheManager.GetAsync(CacheKeys.BlogCategoryCacheKey, async () =>
            {
                return await _context.BlogCategories
                .AsNoTracking()
                .Where(n => n.IsPublished)
                .OrderBy(n => n.Priority)
                .Select(n => new ClientSideBlogCategoryDto()
                {
                    Id = n.Id,
                    Title = n.Title,
                    Slug = n.Slug,
                    MetaDescription = n.MetaDescription
                }).ToListAsync();
            });

            if (!categories?.Any() ?? true)
                return Result.Failure<ClientSideBlogCategoryDto>(Messages.OperationFailedError);

            var category = categories!.FirstOrDefault(n => n.Slug == slug);
            if (category is null)
                return Result.Failure<ClientSideBlogCategoryDto>(Messages.OperationFailedError);

            return new ClientSideBlogCategoryDto()
            {
                Id = category.Id,
                Title = category.Title,
                Slug = category.Slug,
                MetaDescription = category.MetaDescription
            };
        }

        public async Task<Result<List<AdminSideBlogCategoryAsOptionDto>>> AdminSideGetCategoriesAsOptions()
        {
            var categories = await _context.BlogCategories
                .AsNoTracking()
                .Where(n => n.IsPublished)
                .OrderBy(n => n.Priority)
                .Select(n => new AdminSideBlogCategoryAsOptionDto()
                {
                    Id = n.Id,
                    Title = n.Title,
                    Slug = n.Slug
                }).ToListAsync();
            if (!categories?.Any() ?? true) return Result.Failure<List<AdminSideBlogCategoryAsOptionDto>>(Messages.OperationFailedError);

            return categories;
        }

        public async Task<Result> ClientSideAddBlogView(int blogId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return Result.Failure(Messages.OperationFailedError);

            var userId = httpContext.User.GetUserId();
            var userIP = httpContext.GetUserIP();

            bool alreadyViewed;

            if (userId > 0)
            {
                // Logged-in user: check by UserId only
                alreadyViewed = await _context.BlogViews
                    .AnyAsync(n => n.BlogId == blogId && n.UserId == userId);
            }
            else
            {
                alreadyViewed = await _context.BlogViews
                    .AnyAsync(n => n.BlogId == blogId && n.UserId == null && n.UserIP == userIP);
            }

            if (alreadyViewed)
                return Result.Success();

            var view = new BlogView
            {
                BlogId = blogId,
                UserIP = userIP,
                UserId = userId > 0 ? userId : null
            };

            _context.BlogViews.Add(view);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
    }
}