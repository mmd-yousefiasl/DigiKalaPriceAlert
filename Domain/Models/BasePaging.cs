using Microsoft.EntityFrameworkCore;

namespace Domain.Models
{
    public class BasePaging<T>
    {
        public int Page { get; set; }
        public int PageCount { get; set; }
        public int AllEntitiesCount { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public int TakeEntity { get; set; }
        public int SkipEntity { get; set; }
        public int HowManyPagesAfterAndBefore { get; set; }
        public List<T> Entities { get; set; }
        public int Counter { get; set; }

        public BasePaging()
        {
            Page = 1;
            TakeEntity = 10;
            HowManyPagesAfterAndBefore = 5;
            Entities = new List<T>();
        }

        public PagingViewModel GetCurrentPaging()
        {
            return new PagingViewModel()
            {
                EndPage = EndPage,
                Page = Page,
                StartPage = StartPage,
                PageCount = PageCount,
                ShownEntitesTitle = GetShownEntitiesPageTitle()
            };
        }

        public string GetShownEntitiesPageTitle()
        {
            if (AllEntitiesCount != 0)
            {
                int value = 1;
                int value2 = AllEntitiesCount;
                if (EndPage > 1)
                {
                    value = (Page - 1) * TakeEntity + 1;
                    value2 = (Page * TakeEntity > AllEntitiesCount) ? AllEntitiesCount : (Page * TakeEntity);
                }

                return $"در حال نمایش {value} تا {value2} از {AllEntitiesCount} ورودی";
            }

            return "0 آیتم";
        }

        public async Task<BasePaging<T>> Paging(IQueryable<T> queryable)
        {
            int allEntitiesCount = await queryable.CountAsync();

            int pageCount = 0;
            if (allEntitiesCount > 0)
            {
                pageCount = (int)Math.Ceiling((double)allEntitiesCount / TakeEntity);
            }

            Page = (Page > pageCount) ? pageCount : Page;
            if (Page <= 0)
            {
                Page = 1;
            }

            AllEntitiesCount = allEntitiesCount;
            PageCount = pageCount;
            SkipEntity = (Page - 1) * TakeEntity;

            StartPage = (Page - HowManyPagesAfterAndBefore <= 0) ? 1 : (Page - HowManyPagesAfterAndBefore);
            EndPage = (Page + HowManyPagesAfterAndBefore > pageCount) ? pageCount : (Page + HowManyPagesAfterAndBefore);

            Entities = await queryable.Skip(SkipEntity).Take(TakeEntity).ToListAsync();

            Counter = (Page - 1) * TakeEntity + 1;

            return this;
        }
    }
}
