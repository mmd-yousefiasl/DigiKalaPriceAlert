using Domain.Enums.Products;
using Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Products.ClientSide
{
    public class ClientSideFilterProductsDto : BasePaging<ClientSideProductDto>
    {
        [Display(Name = "جست و جو")]
        public string? Search { get; set; }

        [Display(Name = "مرتب‌سازی بر اساس")]
        public ProductSortTypeEnum SortType { get; set; }
    }
}
