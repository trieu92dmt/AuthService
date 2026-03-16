using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Authentication.Infracstructures.CustomAttributes
{
    public class AuthorizationAttribute : ActionFilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userIdentity = context.HttpContext.User.Identity;
            var userName = userIdentity?.Name;

            if (string.IsNullOrEmpty(userName) )
            {
                //Không đăng nhập
                context.Result = new JsonResult(new { code = 401, message = "Unauthorized." }) { StatusCode = 401 };
            }
        }
    }
}
