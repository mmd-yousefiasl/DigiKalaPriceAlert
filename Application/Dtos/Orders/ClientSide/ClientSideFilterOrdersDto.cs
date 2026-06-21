using Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos.Orders.ClientSide
{
    public class ClientSideFilterOrdersDto : BasePaging<ClientSideOrderDto>
    {
        [Display(Name = "جست و جو")]
        public string? Search { get; set; }
    }
}
