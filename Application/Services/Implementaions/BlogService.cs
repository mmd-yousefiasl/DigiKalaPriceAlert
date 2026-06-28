using Application.Dtos.Blog.AdminSide;
using Application.Dtos.Blog.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Models.Blogs;
using Domain.Shared;
using Domain.Statics;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class BlogService : IBlogService
    {
        #region Fields
        private readonly DigiKalaPriceAlertDbContext _context;
        #endregion

        #region Ctor
        public BlogService(DigiKalaPriceAlertDbContext context)
        {
            _context = context;
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
                Slug = slug
            };

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

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

            var blog = await _context.Blogs.Where(n => n.Id == id)
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
                }).AsNoTracking().FirstOrDefaultAsync();
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

            return Result.Success();
        }

        public async Task<Result<ClientSideFilterBlogsDto>> ClientSideFilterBlogs(ClientSideFilterBlogsDto filter)
        {
            if (filter is null) return Result.Failure<ClientSideFilterBlogsDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<Blog>();

            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                conditions.Add(n => n.Category.Slug == filter.Category);
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

            var blog = await _context.Blogs.Where(n => n.Id == id && n.IsPublished)
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
                    ViewsCount = n.Views.Count()
                }).AsNoTracking().FirstOrDefaultAsync();
            if (blog is null)
                return Result.Failure<ClientSideBlogDetailsDto>(Messages.OperationFailedError);

            return blog;
        }

        public async Task<Result<List<ClientSideBlogDto>>> ClientSideGetHighPriorityBlogs(int take)
        {
            if (take < 1) return Result.Failure<List<ClientSideBlogDto>>(Messages.OperationFailedError);

            var blogs = await _context.Blogs.Where(n => n.IsPublished).OrderBy(n => n.Priority).Take(take)
                .Select(n => new ClientSideBlogDto()
                {
                    Id = n.Id,
                    Title = n.Title,
                    Slug = n.Slug,
                    CreatedDate = n.CreatedDate,
                    FileName = n.FileName
                }).AsNoTracking().ToListAsync();
            if (!blogs.Any())
                return Result.Failure<List<ClientSideBlogDto>>(Messages.OperationFailedError);

            return blogs;
        }

        public async Task<Result<List<ClientSideBlogDto>>> ClientSideGetRelatedBlogs(int blogId, int categoryId)
        {
            var blogs = await _context.Blogs.Where(n => n.IsPublished && n.Id != blogId && n.CategoryId == categoryId)
                .OrderBy(n => n.Priority)
                .Take(5)
                .Select(n => new ClientSideBlogDto()
                {
                    Id = n.Id,
                    CreatedDate = n.CreatedDate,
                    Slug = n.Slug,
                    FileName = n.FileName,
                    Title = n.Title
                }).AsNoTracking().ToListAsync();
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
    }
}
