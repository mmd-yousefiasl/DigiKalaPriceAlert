using Application.Dtos.Products.ClientSide;
using Application.Services.Interfaces;
using Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Web.Attributes;

namespace Web.Areas.UserPanel.Controllers
{
    [Membership]
    public class ProductController : BaseUserPanelController
    {
        #region Fields
        private readonly IProductService _productService;
        #endregion

        #region Ctor
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        #endregion

        [HttpGet("/user/products/list")]
        public async Task<IActionResult> ProductsList(ClientSideFilterProductsDto filter)
        {
            filter.TakeEntity = 20;
            var result = await _productService.ClientSideFilterProducts(filter);
            if (result.IsFailure)
                return View(new ClientSideFilterProductsDto());

            return View(result.Value);
        }

        [Profile]
        [HttpGet("/user/products/add")]
        public IActionResult AddNewProduct()
        {
            return View();
        }

        [Profile]
        [HttpPost("/user/products/add"), ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewProduct(ClientSideCreateNewProductDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _productService.ClientSideCreateNewProduct(dto);
            if (result.IsFailure)
            {
                TempData[ErrorMessage] = result.Message;
                return View(dto);
            }
            TempData[TemporarySuccessMessage] = result.Message;

            return RedirectToAction(nameof(ProductDetails), new { id = result.Value });
        }

        [HttpGet("/user/products/{id}")]
        public async Task<IActionResult> ProductDetails(int id)
        {
            if (id < 1)
            {
                TempData[TemporaryErrorMessage] = Messages.OperationFailedError;
                return RedirectToAction(nameof(ProductsList));
            }

            var result = await _productService.ClientSideGetProductDetails(id);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return RedirectToAction(nameof(ProductsList));
            }
            await _productService.ClientSideSetProductAsReadForUser(result.Value.Id);

            return View(result.Value);
        }

        [HttpPost("/user/products/remove/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (id < 1)
            {
                TempData[TemporaryErrorMessage] = Messages.OperationFailedError;
                return RedirectToAction(nameof(ProductsList));
            }
            var result = await _productService.ClientSideDeleteUserProduct(id);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
            }
            else
            {
                TempData[TemporarySuccessMessage] = result.Message;
            }

            return RedirectToAction(nameof(ProductsList));
        }
    }
}
