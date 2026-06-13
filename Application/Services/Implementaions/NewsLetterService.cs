using Application.Dtos.NewsLetter.AdminSide;
using Application.Dtos.NewsLetter.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Models.NewsLetters;
using Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class NewsLetterService : INewsLetterService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Ctor
        public NewsLetterService(DigiKalaPriceAlertDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        public async Task<Result> ClientSideSignInNewsLetters(ClientSideSignInNewsLettersDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);

            if (await _context.NewsLetters.AnyAsync(n => n.Email == model.Email))
                return Result.Failure(Messages.ThisEmailAlreadyRegisterdError);

            NewsLetter newsLetter = new NewsLetter()
            {
                Email = model.Email,
                UserIP = _httpContextAccessor.HttpContext.GetUserIP(),
            };

            var userId = _httpContextAccessor.HttpContext.User?.GetUserId() ?? null;
            if (userId is null || userId is 0)
            {
                newsLetter.UserId = null;
            }
            else
            {
                newsLetter.UserId = userId;
            }

            _context.NewsLetters.Add(newsLetter);
            await _context.SaveChangesAsync();

            return Result.Success(Messages.SignInNewsLettersSuccessfully);
        }

        public async Task<Result<AdminSideFilterNewsLettersDto>> AdminSideFilterNewsLetters(AdminSideFilterNewsLettersDto filter)
        {
            if (filter is null)
                return Result.Failure<AdminSideFilterNewsLettersDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<NewsLetter>();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.Email.Contains(filter.Search) ||
                n.UserIP.Contains(filter.Search));
            }

            IQueryable<NewsLetter> query = _context.NewsLetters;
            query = query.OrderByDescending(n => n.Id);

            foreach (var item in conditions ?? new FilterConditions<NewsLetter>())
            {
                query = query.Where(item);
            }

            await filter.Paging(query.Select(n => new AdminSideNewsLetterDto()
            {
                Id = n.Id,
                Email = n.Email,
                UserIP = n.UserIP
            }));

            return filter;
        }

        public async Task<Result> AdminSideDeleteNewsLetter(int id)
        {
            if (id < 1) return Result.Failure(Messages.OperationFailedError);

            var newsLetter = await _context.NewsLetters.FindAsync(id);
            if (newsLetter is null) return Result.Failure(Messages.OperationFailedError);

            newsLetter.IsDeleted = true;
            newsLetter.LastUpdatedDate = DateTime.UtcNow;

            _context.NewsLetters.Update(newsLetter);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
    }
}
