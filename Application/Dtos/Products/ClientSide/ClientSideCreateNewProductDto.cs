using Domain.Shared;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideCreateNewProductDto
    {
        [Display(Name = "لینک یا کد کالا")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(1000, ErrorMessage = Messages.MaxLengthError)]
        public string Url { get; set; }

        [Display(Name = "کد فروشندگی")]
        [Required(ErrorMessage = Messages.RequiredError)]
        [MaxLength(50, ErrorMessage = Messages.MaxLengthError)]
        public string SellerCode { get; set; }
    }
}
