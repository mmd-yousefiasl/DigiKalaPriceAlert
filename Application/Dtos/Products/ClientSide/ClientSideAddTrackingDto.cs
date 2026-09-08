using Domain.Shared;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideAddTrackingDto
    {
        [Display(Name = "شناسه فروشنده")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [Range(1, int.MaxValue, ErrorMessage = Messages.SellerNotFoundError)]
        public int SellerId { get; set; }

        [Display(Name = "شناسه فروشنده دیجی کالا")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [Range(1, long.MaxValue, ErrorMessage = Messages.SellerNotFoundError)]
        public long DigiKalaVariantId { get; set; }
    }
}
