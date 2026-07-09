using Application.Dtos.Medium.AdminSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.Attributes;

namespace Web.Areas.Admin.Controllers
{
    [Permission("ManageMedia")]
    public class MediaController : BaseAdminController
    {
        #region Fields
        private readonly IMediumService _mediumService;
        #endregion

        #region Ctor
        public MediaController(IMediumService mediumService)
        {
            _mediumService = mediumService;
        }
        #endregion

        [Permission("MediaList")]
        [HttpGet("/admin/media-list")]
        public async Task<IActionResult> MediaList(AdminSideFilterMediaDto filter)
        {
            var result = await _mediumService.AdminSideFilterMedia(filter);
            if (result.IsFailure) return PartialView("_MediaList", new AdminSideFilterMediaDto());

            return PartialView("_MediaList", result.Value);
        }

        [Permission("CreateNewMedium")]
        [HttpPost("/admin/add-new-medium"), ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNewMedium(AdminSideCreateNewMediumDto model)
        {
            var result = await _mediumService.AdminSideCreateNewMedium(model);

            if (result.IsSuccess)
            {
                return Json(new { success = true, message = result.Message });
            }

            return Json(new { success = false, message = result.Message });
        }

        [Permission("DeleteMedium")]
        [HttpPost("/admin/remove-medium"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedium(int id)
        {
            var result = await _mediumService.AdminSideDeleteMedium(id);

            if (result.IsSuccess)
            {
                return Json(new { success = true, message = result.Message });
            }

            return Json(new { success = false, message = result.Message });
        }
    }
}
