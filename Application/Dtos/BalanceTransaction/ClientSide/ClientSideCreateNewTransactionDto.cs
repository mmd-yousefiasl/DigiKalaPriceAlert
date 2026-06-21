using Domain.Enums.Users;

namespace Application.Dtos.BalanceTransaction.ClientSide
{
    public class ClientSideCreateNewTransactionDto
    {
        public decimal Amount { get; set; }
        public BalanceTransactionTypeEnum Type { get; set; }
        public string? Description { get; set; }
        public bool IsSuccess { get; set; }
        public string? TransId { get; set; }
    }
}
