using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Models.Blogs;
using Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class BlogViewService : IBlogViewService
    {
        #region Fields
        private readonly DigiKalaPriceAlertDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Ctor
        public BlogViewService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        public async Task<Result> ClientSideAddBlogView(int blogId)
        {
            var userId = _httpContextAccessor.HttpContext.User.GetUserId();
            var userIP = _httpContextAccessor.HttpContext.GetUserIP();

            if (await _context.BlogViews.AnyAsync(n => (n.UserId == userId || n.UserIP == userIP) && n.BlogId == blogId))
                return Result.Failure(Messages.OperationFailedError);

            BlogView view = new BlogView()
            {
                BlogId = blogId,
                UserIP = userIP
            };
            if (userId > 0) view.UserId = userId;

            _context.BlogViews.Add(view);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
    }
}