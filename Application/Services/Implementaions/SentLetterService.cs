using Application.Dtos.SentLetter.AdminSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Models.NewsLetters;
using Domain.Shared;

namespace Application.Services.Implementaions
{
    public class SentLetterService : ISentLetterService
    {
        #region Fields
        private protected DigiKalaPriceAlertDbContext _context;
        #endregion

        #region Ctor
        public SentLetterService(DigiKalaPriceAlertDbContext context)
        {
            _context = context;
        }
        #endregion

        public async Task<Result<AdminSideFilterSentLettersDto>> AdminSideFilterSentLetters(AdminSideFilterSentLettersDto filter)
        {
            if (filter is null)
                return Result.Failure<AdminSideFilterSentLettersDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<SentLetter>();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.Subject.Contains(filter.Search) ||
                n.Message.Contains(filter.Search));
            }

            if (filter.IsSucceed is not null)
            {
                conditions.Add(n => n.IsSucceed == filter.IsSucceed);
            }

            IQueryable<SentLetter> query = _context.SentLetters;
            query = query.OrderByDescending(n => n.Id);

            foreach (var condition in conditions ?? new FilterConditions<SentLetter>())
            {
                query = query.Where(condition);
            }

            await filter.Paging(query.Select(n => new AdminSideSentLetterDto()
            {
                Id = n.Id,
                SentDate = n.CreatedDate,
                Subject = n.Subject,
                IsSucceed = n.IsSucceed,
                Message = n.Message,
                TargetMailCount = n.TargetMailCount
            }));

            return filter;
        }

        public async Task<Result> AdminSideDeleteSentLetter(int id)
        {
            if (id < 1) return Result.Failure(Messages.OperationFailedError);

            var letter = await _context.SentLetters.FindAsync(id);
            if (letter is null) return Result.Failure(Messages.OperationFailedError);

            letter.LastUpdatedDate = DateTime.UtcNow;
            letter.IsDeleted = true;

            _context.SentLetters.Update(letter);
            await _context.SaveChangesAsync();

            return Result.Success(Messages.SuccessfullyDone);
        }

        public async Task<Result> AdminSideCreateNewSentLetter(AdminSideCreateNewSentLetterDto dto)
        {
            if (dto is null) return Result.Failure(Messages.OperationFailedError);

            SentLetter letter = new SentLetter()
            {
                Subject = dto.Subject,
                Message = dto.Message,
                TargetMailCount = dto.TargetMailCount,
                IsSucceed = dto.IsSucceed,
            };

            _context.SentLetters.Add(letter);
            await _context.SaveChangesAsync();

            return Result.Success(Messages.SuccessfullyDone);
        }
    }
}
