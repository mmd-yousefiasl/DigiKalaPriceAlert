using Application.Dtos.Media.ClientSide;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Components
{
    public class SocialMediaViewComponent : ViewComponent
    {
        private readonly ISocialMediaService _mediaService;
        public SocialMediaViewComponent(ISocialMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var result = await _mediaService.ClientSideGetSocialMedia();
            if (result.IsFailure) return View("SocialMedia", new List<ClientSideSocialMediaDto>());

            return View("SocialMedia", result.Value);
        }
    }
}
