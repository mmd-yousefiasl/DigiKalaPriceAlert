using Domain.Models.Orders;

namespace Domain.Models.Users
{
    public class User : AuditBaseEntity<int>
    {
        public string Mobile { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public bool IsUserBanned { get; set; } = false;
        public decimal Balance { get; set; }

        #region Relations
        public ICollection<UserProductMapping>? Products { get; set; }

        public ICollection<UserRoleMapping>? UserRoleMapping { get; set; }
        public ICollection<BalanceTransaction>? Transactions { get; set; }

        public ICollection<Order>? Orders { get; set; }
        #endregion
    }
}
