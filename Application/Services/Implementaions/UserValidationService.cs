using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Models.Users;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class UserValidationService : IUserValidationService
    {
        #region Fields
        private readonly DigiKalaPriceAlertDbContext _context;
        #endregion

        #region Ctor
        public UserValidationService(DigiKalaPriceAlertDbContext context)
        {
            _context = context;
        }
        #endregion

        public async Task<Result<string>> GenerateUserValidationCode(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return Result.Failure<string>(Messages.OperationFailedError);

            var existingValidations = await _context.UserValidation
                .Where(uv => uv.Mobile == mobile)
                .OrderByDescending(v => v.CreatedDate)
                .ToListAsync();

            var latestValidation = existingValidations.FirstOrDefault();
            if (latestValidation != null &&
                DateTime.UtcNow - latestValidation.CreatedDate < TimeSpan.FromMinutes(2))
            {
                return Result.Failure<string>(Messages.ToManyLoginAttemptError);
            }

            if (existingValidations.Any())
                _context.UserValidation.RemoveRange(existingValidations);

            var code = Generate.UserLoginCode();
            var hashedCode = code.HashPassword(out string salt);

            var validation = new UserValidation
            {
                Code = hashedCode,
                Salt = salt,
                Mobile = mobile,
                CreatedDate = DateTime.UtcNow,
            };

            _context.UserValidation.Add(validation);
            await _context.SaveChangesAsync();

            return code;
        }

        public async Task<Result> ValidateUserLoginCode(string mobile, string code)
        {
            if (string.IsNullOrWhiteSpace(mobile) || string.IsNullOrWhiteSpace(code)) return Result.Failure(Messages.OperationFailedError);

            var validation = await _context.UserValidation.Where(uv => uv.Mobile == mobile)
                .FirstOrDefaultAsync();
            if (validation is null)
                return Result.Failure(Messages.OperationFailedError);

            if (!code.VerifyPassword(validation.Salt, validation.Code)) return Result.Failure(Messages.InvalidLoginCodeError);

            _context.UserValidation.Remove(validation);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
    }
}
