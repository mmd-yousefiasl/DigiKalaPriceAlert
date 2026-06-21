using Application.Dtos.Orders.AdminSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.Attributes;

namespace Web.Areas.Admin.Controllers
{
    [Permission("ManageOrders")]
    public class OrderController : BaseAdminController
    {
        #region Fields
        private readonly IOrderService _orderService;
        #endregion

        #region Ctor
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        #endregion

        [Permission("OrdersList")]
        [HttpGet("/admin/orders-list")]
        public async Task<IActionResult> OrdersList(AdminSideFilterOrdersDto filter)
        {
            var result = await _orderService.AdminSIdeFilterOrders(filter);
            if (result.IsFailure)
                return View(new AdminSideFilterOrdersDto());

            return View(result.Value);
        }
    }
}
