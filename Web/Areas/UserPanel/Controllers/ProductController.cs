using Microsoft.AspNetCore.Mvc;

namespace Web.Areas.UserPanel.Controllers
{
    public class ProductController : BaseUserPanelController
    {
        #region Fields

        #endregion

        #region Ctor

        #endregion

        [HttpGet("/user/products/list")]
        public async Task<IActionResult> ProductsList()
        {
            return View();
        }

        [HttpGet("/user/products/add")]
        public IActionResult AddNewProduct()
        {
            return View();
        }

        [HttpGet("/user/products/{id}")]
        public async Task<IActionResult> ProductDetails(int id)
        {
            return View();
        }
    }
}
