using Infrastructure.Interfaces.Services;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebAPI.AuthLayer
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthenticateAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var authHeader = httpContext.Request.Headers["Authorization"].ToString();
            var token = TokenHelpers.NormalizeToken(authHeader);

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var tokenService = httpContext.RequestServices.GetRequiredService<IUserServices>();
            var userId = await tokenService.GetUserIdByTokenAsync(token);

            if (userId == null)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Invalid or revoked token" });
                return;
            }

            //var ttl = TokenHelpers.GetTtlFromToken(token);
            //if (ttl <= TimeSpan.Zero)
            //{
            //    context.Result = new UnauthorizedObjectResult(new { message = "Token expired" });
            //    return;
            //}

            httpContext.Items["UserId"] = userId.Value;

            await next();
        }
    }

}
