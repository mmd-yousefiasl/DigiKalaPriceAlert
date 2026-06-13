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
    }
}
