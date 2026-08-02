using Domain.Shared;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideSearchProductDto
    {
        [Display(Name = "لینک یا کد کالا")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(1000, ErrorMessage = Messages.MaxLengthError)]
        public string Url { get; set; } = string.Empty;
    }

    public class ClientSideAddTrackingDto
    {
        [Display(Name = "شناسه فروشنده")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [Range(1, int.MaxValue, ErrorMessage = Messages.SellerNotFoundError)]
        public int SellerId { get; set; }
    }
}
