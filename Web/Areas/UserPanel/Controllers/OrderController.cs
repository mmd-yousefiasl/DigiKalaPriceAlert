using Application.Dtos.Orders.ClientSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.UserPanel.Controllers
{
    public class OrderController : BaseUserPanelController
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

        [HttpGet("/user/orders")]
        public async Task<IActionResult> OrdersList(ClientSideFilterOrdersDto filter)
        {
            var result = await _orderService.ClientSideFilterOrders(filter);
            if (result.IsFailure)
                return View(new ClientSideFilterOrdersDto());

            return View(result.Value);
        }
    }
}