using Application.Dtos.Users.BotSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Models.Users;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class UserService : IUserService
    {
        #region Fields
        private readonly DigiKalaPriceAlertDbContext _context;
        #endregion

        #region Ctor
        public UserService(DigiKalaPriceAlertDbContext context)
        {
            _context = context;
        }
        #endregion

        public async Task<Result> BotSideCanUserAddProduct(long chatId)
        {
            if (await _context.UserProductMapping.CountAsync(up => up.User.ChatId == chatId) > AppInfo.MaxUserProducts)
                return Result.Failure(BotMessages.CantAddMoreProductsError);

            return Result.Success();
        }

        public async Task<Result> BotSideCreateNewUser(BotSideCreateNewUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.ChatId == dto.ChatId))
                return Result.Failure(BotMessages.UserAlreadyExistMessage);

            var mobileResult = dto.Mobile.NormalizeIranPhoneNumber();
            if (mobileResult.IsFailure)
                return Result.Failure(mobileResult.Message!);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Mobile == mobileResult.Value);
            if (user != null)
            {
                user.ChatId = dto.ChatId;
                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.Username = dto.UserName;

                _context.Users.Update(user);
            }
            else
            {
                user = new User()
                {
                    Mobile = mobileResult.Value,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Username = dto.UserName,
                    ChatId = dto.ChatId,
                };

                _context.Users.Add(user);
            }

            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> BotSideCreateNewUserProduct(BotSideCreateNewUserProductDto dto)
        {
            if (await _context.UserProductMapping.AnyAsync(up => up.User.ChatId == dto.ChatId && up.ProductId == dto.ProductId))
                return Result.Failure(BotMessages.ProductAlreadyInWatchListError);

            var userIdResult = await GetUserIdByChatId(dto.ChatId);
            if (userIdResult.IsFailure)
                return Result.Failure(userIdResult.Message!);

            var userProduct = new UserProductMapping
            {
                UserId = userIdResult.Value,
                ProductId = dto.ProductId,
            };
            _context.Add(userProduct);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> BotSideDoesUserExist(long chatId)
        {
            if (await _context.Users.AnyAsync(u => u.ChatId == chatId))
                return Result.Success();

            return Result.Failure(Messages.UserDoesNotExistError);
        }

        public async Task<List<long>> BotSideGetUserChatIdsTrackingThisProduct(BotSideGetUsersTrackingProductDto dto)
        {
            return await _context.UserProductMapping
                .AsNoTracking()
                .Where(upm => upm.ProductId == dto.ProductId && upm.User.ChatId != null)
                .Select(upm => upm.User.ChatId!.Value)
                .ToListAsync(dto.CancellationToken);
        }

        public async Task<Result<int>> GetUserIdByChatId(long chatId)
        {
            var userId = await _context.Users
                .Where(u => u.ChatId == chatId)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();
            if (userId < 1) return Result.Failure<int>(Messages.OperationFailedError);

            return userId;
        }
    }
}
