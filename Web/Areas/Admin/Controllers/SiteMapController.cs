using Application.Dtos.SiteMap.AdminSide;
using Application.Services.Interfaces;
using Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Web.Attributes;

namespace Web.Areas.Admin.Controllers
{
    [Permission("ManageSiteMap")]
    public class SiteMapController : BaseAdminController
    {
        #region Fields
        private readonly ISiteMapService _mapService;
        #endregion

        #region Ctor
        public SiteMapController(ISiteMapService mapService)
        {
            _mapService = mapService;
        }
        #endregion

        [Permission("SiteMapList")]
        [HttpGet("/admin/site-map-list")]
        public async Task<IActionResult> SiteMapList(AdminSideFilterSiteMapDto filter)
        {
            var result = await _mapService.AdminSideFilterSiteMap(filter);
            if (result.IsFailure) return View(new AdminSideFilterSiteMapDto());

            return View(result.Value);
        }

        [Permission("CreateNewSiteMap")]
        [HttpGet("/admin/add-site-map")]
        public IActionResult CreateNewSiteMap()
        {
            return View();
        }

        [Permission("CreateNewSiteMap")]
        [HttpPost("/admin/add-site-map"), ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNewSiteMap(AdminSideCreateNewSiteMapDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _mapService.AdminSideCreateNewSiteMap(model);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return View(model);
            }
            TempData[TemporarySuccessMessage] = result.Message;

            return RedirectToAction(nameof(SiteMapList));
        }

        [Permission("UpdateSiteMap")]
        [HttpGet("/admin/edit-site-map")]
        public async Task<IActionResult> UpdateSiteMap(int id)
        {
            if (id < 1)
            {
                TempData[TemporaryErrorMessage] = Messages.OperationFailedError;
                return RedirectToAction(nameof(SiteMapList));
            }
            var result = await _mapService.AdminSideGetSiteMapForUpdate(id);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return RedirectToAction(nameof(SiteMapList));
            }

            return View(result.Value);
        }

        [Permission("UpdateSiteMap")]
        [HttpPost("/admin/edit-site-map"), ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSiteMap(AdminSideUpdateSiteMapDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _mapService.AdminSideUpdateSiteMap(model);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return View(model);
            }
            TempData[TemporarySuccessMessage] = result.Message;

            return RedirectToAction(nameof(SiteMapList));
        }

        [Permission("DeleteSiteMap")]
        [HttpPost("/admin/remove-site-map"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSiteMap(int id)
        {
            if (id < 1)
            {
                TempData[TemporaryErrorMessage] = Messages.OperationFailedError;
                return RedirectToAction(nameof(SiteMapList));
            }
            var result = await _mapService.AdminSideDeleteSiteMap(id);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
            }
            else
            {
                TempData[TemporarySuccessMessage] = result.Message;
            }

            return RedirectToAction(nameof(SiteMapList));
        }

        [Permission("UpdateSiteMap")]
        [HttpPost("/admin/edit-all-links"), ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAllLinks()
        {
            var result = await _mapService.UpdateAllSiteMap();
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
            }
            else
            {
                TempData[TemporarySuccessMessage] = result.Message;
            }

            return RedirectToAction(nameof(SiteMapList));
        }

        #region Bulk Delete
        [Permission("UpdateSiteMap")]
        [HttpPost("/admin/remove-all-links"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllLinks()
        {
            var result = await _mapService.AdminSideDeleteAllLinks();
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
            }
            else
            {
                TempData[TemporarySuccessMessage] = result.Message;
            }

            return RedirectToAction(nameof(SiteMapList));
        }

        [Permission("UpdateSiteMap")]
        [HttpPost("/admin/remove-all-except-other-links"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllExceptOtherPagesLinks()
        {
            var result = await _mapService.AdminSideDeleteAllExceptOtherPages();
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
            }
            else
            {
                TempData[TemporarySuccessMessage] = result.Message;
            }

            return RedirectToAction(nameof(SiteMapList));
        }
        #endregion

        #region Bulk Update Blog
        [Permission("UpdateSiteMap")]
        [HttpGet("/admin/edit-blog-links")]
        public async Task<IActionResult> UpdateBlogLinks()
        {
            var result = await _mapService.UpdateBlogsSiteMap();
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
            }
            else
            {
                TempData[TemporarySuccessMessage] = result.Message;
            }

            return RedirectToAction(nameof(SiteMapList));
        }

        [Permission("UpdateSiteMap")]
        [HttpGet("/admin/edit-blog-category-links")]
        public async Task<IActionResult> UpdateBlogCategoryLinks()
        {
            var result = await _mapService.UpdateBlogCategoriesSiteMap();
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
            }
            else
            {
                TempData[TemporarySuccessMessage] = result.Message;
            }

            return RedirectToAction(nameof(SiteMapList));
        }
        #endregion
    }
}
