using Domain.Enums.Users;

namespace Application.Dtos.BalanceTransaction.AdminSide
{
    public class AdminSideBalanceTransactionDto : CommonDto
    {
        public DateTime CreatedDate { get; set; }
        public decimal Amount { get; set; }
        public BalanceTransactionTypeEnum Type { get; set; }
        public string? Description { get; set; }
        public bool IsSuccess { get; set; }
        public string? TransId { get; set; }
    }
}
