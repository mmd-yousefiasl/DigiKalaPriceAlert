using Application.Services.Interfaces;
using Application.Tools;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Web.TagHelpers
{
    [HtmlTargetElement(Attributes = "permission")]
    public class PermissionCheckerTagHelper : TagHelper
    {
        #region Fields
        private readonly IRoleService _roleService;
        #endregion

        #region Ctor

        public PermissionCheckerTagHelper(IRoleService roleService)
        {
            _roleService = roleService;
        }

        #endregion

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContextData { get; set; }

        [HtmlAttributeName("permission")]
        public string PermissionName { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!ViewContextData.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                output.SuppressOutput();
                return;
            }

            if (string.IsNullOrWhiteSpace(PermissionName))
            {
                output.SuppressOutput();
                return;
            }

            var permissions = PermissionName
                .Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(n => n.Trim())
                .ToList();

            if (!permissions.Any())
            {
                output.SuppressOutput();
                return;
            }

            var userId = ViewContextData.HttpContext.User.GetUserId();

            foreach (var permission in permissions)
            {
                var result = await _roleService.CheckUserAccess(userId, permission);

                if (result.IsSuccess)
                {
                    return;
                }
            }

            output.SuppressOutput();
        }
    }
}
