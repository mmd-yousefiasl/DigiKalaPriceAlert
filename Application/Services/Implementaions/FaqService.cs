using Application.Dtos.Faq.AdminSide;
using Application.Dtos.Faq.ClientSide;
using Application.Services.Interfaces;
using Application.Tools;
using Data.Context;
using Domain.Models.Faqs;
using Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementaions
{
    public class FaqService : IFaqService
    {
        #region Fields
        private readonly DigiKalaPriceAlertDbContext _context;
        #endregion

        #region Ctor
        public FaqService(DigiKalaPriceAlertDbContext context)
        {
            _context = context;
        }
        #endregion

        public async Task<Result> CreateNewFaq(AdminSideCreateNewFaqDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);

            Faq faq = new Faq()
            {
                Question = model.Question,
                Answer = model.Answer,
                Priority = model.Priority,
                IsPublished = model.IsPublished
            };

            _context.Faqs.Add(faq);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result<AdminSideFilterFaqsDto>> FilterFaqs(AdminSideFilterFaqsDto filter)
        {
            if (filter is null) return Result.Failure<AdminSideFilterFaqsDto>(Messages.OperationFailedError);

            var conditions = Filter.GenerateConditions<Faq>();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                conditions.Add(n => n.Question.Contains(filter.Search) || n.Answer.Contains(filter.Search));
            }

            if (filter.IsPublished is not null)
            {
                conditions.Add(n => n.IsPublished == filter.IsPublished);
            }

            IQueryable<Faq> query = _context.Faqs;
            query = query.OrderByDescending(n => n.Id);

            foreach (var item in conditions ?? new FilterConditions<Faq>())
            {
                query = query.Where(item);
            }

            await filter.Paging(query.Select(n => new AdminSideFaqDto()
            {
                Id = n.Id,
                Question = n.Question,
                Answer = n.Answer,
                Priority = n.Priority,
                IsPublished = n.IsPublished
            }));

            return filter;
        }

        public async Task<Result<AdminSideUpdateFaqDto>> GetFaqForUpdate(int id)
        {
            if (id < 1) return Result.Failure<AdminSideUpdateFaqDto>(Messages.OperationFailedError);

            var faq = await _context.Faqs.Where(n => n.Id == id).Select(n => new AdminSideUpdateFaqDto()
            {
                Id = n.Id,
                Question = n.Question,
                Answer = n.Answer,
                IsPublished = n.IsPublished,
                Priority = n.Priority
            }).AsNoTracking().FirstOrDefaultAsync();
            if (faq is null)
                return Result.Failure<AdminSideUpdateFaqDto>(Messages.OperationFailedError);

            return faq;
        }

        public async Task<Result<List<ClientSideFaqDto>>> GetFaqs()
        {
            var faqs = await _context.Faqs.Where(n => n.IsPublished).AsNoTracking().ToListAsync();
            if (!faqs?.Any() ?? true) return Result.Failure<List<ClientSideFaqDto>>(Messages.OperationFailedError);

            return faqs.OrderBy(n => n.Priority).Select(n => new ClientSideFaqDto()
            {
                Answer = n.Answer,
                Question = n.Question
            }).ToList();
        }

        public async Task<Result> SoftDeleteFaq(int id)
        {
            if (id < 1) return Result.Failure(Messages.OperationFailedError);

            var faq = await _context.Faqs.FindAsync(id);
            if (faq is null) return Result.Failure(Messages.OperationFailedError);

            faq.IsDeleted = true;

            _context.Faqs.Update(faq);
            await _context.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> UpdateFaq(AdminSideUpdateFaqDto model)
        {
            if (model is null) return Result.Failure(Messages.OperationFailedError);

            var faq = await _context.Faqs.FindAsync(model.Id);
            if (faq is null) return Result.Failure(Messages.OperationFailedError);

            faq.Question = model.Question;
            faq.Answer = model.Answer;
            faq.Priority = model.Priority;
            faq.IsPublished = model.IsPublished;

            _context.Faqs.Update(faq);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
    }
}
