using Application.Dtos.Media.AdminSide;
using Application.Services.Interfaces;
using Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Web.Attributes;

namespace Web.Areas.Admin.Controllers
{
    [Permission("ManageSocialMedia")]
    public class SocialMediaController : BaseAdminController
    {
        #region Fields
        private readonly ISocialMediaService _mediaService;
        #endregion

        #region  Ctor
        public SocialMediaController(ISocialMediaService mediaService)
        {
            _mediaService = mediaService;
        }
        #endregion

        [Permission("SocialMediaList")]
        [HttpGet("/admin/social-media-list")]
        public async Task<IActionResult> SocialMediaList(AdminSideFilterSocialMediaDto filter)
        {
            var result = await _mediaService.AdminSideFilterSocialMedia(filter);
            if (result.IsFailure) return View(new AdminSideFilterSocialMediaDto());

            return View(result.Value);
        }

        [Permission("CreateNewSocialMedia")]
        [HttpGet("/admin/add-new-social-media")]
        public IActionResult CreateNewSocialMedia()
        {
            return View();
        }

        [Permission("CreateNewSocialMedia")]
        [HttpPost("/admin/add-new-social-media"), ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNewSocialMedia(AdminSideCreateNewSocialMediaDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _mediaService.AdminSideCreateNewSocialMedia(model);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return View(model);
            }
            TempData[TemporarySuccessMessage] = result.Message;

            return RedirectToAction(nameof(SocialMediaList));
        }

        [Permission("UpdateSocialMedia")]
        [HttpGet("/admin/edit-social-media")]
        public async Task<IActionResult> UpdateSocialMedia(int id)
        {
            if (id < 1)
            {
                TempData[TemporaryErrorMessage] = Messages.OperationFailedError;
                return RedirectToAction(nameof(SocialMediaList));
            }
            var result = await _mediaService.AdminSideGetSocialMediaForUpdate(id);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return RedirectToAction(nameof(SocialMediaList));
            }

            return View(result.Value);
        }

        [Permission("UpdateSocialMedia")]
        [HttpPost("/admin/edit-social-media"), ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSocialMedia(AdminSideUpdateSocialMediaDto model)
        {
            if (!ModelState.IsValid) return View(model);
            var result = await _mediaService.AdminSideUpdateSocialMedia(model);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
                return View(model);
            }
            TempData[TemporarySuccessMessage] = result.Message;

            return RedirectToAction(nameof(SocialMediaList));
        }

        [Permission("DeleteSocialMedia")]
        [HttpPost("/admin/remove-social-media"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSocialMedia(int id)
        {
            if (id < 1)
            {
                TempData[TemporaryErrorMessage] = Messages.OperationFailedError;
                return RedirectToAction(nameof(SocialMediaList));
            }
            var result = await _mediaService.AdminSideDeleteSocialMedia(id);
            if (result.IsFailure)
            {
                TempData[TemporaryErrorMessage] = result.Message;
            }
            else
            {
                TempData[TemporarySuccessMessage] = result.Message;
            }

            return RedirectToAction(nameof(SocialMediaList));
        }
    }
}
