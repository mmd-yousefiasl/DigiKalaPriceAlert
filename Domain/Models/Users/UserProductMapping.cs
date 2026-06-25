using Domain.Models.Products;

namespace Domain.Models.Users
{
    public class UserProductMapping
    {
        public bool IsRead { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
