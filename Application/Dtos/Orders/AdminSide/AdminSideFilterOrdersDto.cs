using Domain.Enums.Orders;
using Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Orders.AdminSide
{
    public class AdminSideFilterOrdersDto : BasePaging<AdminSideOrderDto>
    {
        [Display(Name = "جست و جو")]
        public string? Search { get; set; }

        [Display(Name = "وضعیت")]
        public OrderStatusEnum? Status { get; set; }
    }
}
