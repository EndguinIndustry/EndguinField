using System.Security.Claims;
using EndguinField.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace EndguinField.Middleware
{
    public class BannedUserMiddleware
    {
        private readonly RequestDelegate _next;

        public BannedUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserService userService)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    var path = context.Request.Path.Value?.ToLower();
                    if (path != null && !path.Contains("/login/logout") && !path.Contains("/login/banned"))
                    {
                        var user = await userService.GetUserByIdAsync(userId);
                        if (user != null && user.BanUntil.HasValue && user.BanUntil > DateTime.Now)
                        {
                            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            
                            context.Response.Redirect("/Login/Banned?until=" + user.BanUntil.Value.ToString("O"));
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }

    public static class BannedUserMiddlewareExtensions
    {
        public static IApplicationBuilder UseBannedUserCheck(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BannedUserMiddleware>();
        }
    }
}
