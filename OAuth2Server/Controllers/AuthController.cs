using Infracstructure.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OAuth2Server.Applications.DTOs.Auth;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;

namespace OAuth2Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateAccountRequest req)
        {
            var exist = await _userManager.FindByNameAsync(req.Username);
            if (exist != null)
                return BadRequest("Username already exists");

            var user = new ApplicationUser
            {
                UserName = req.Username
            };

            var result = await _userManager.CreateAsync(user, req.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Register success");
        }

        [HttpGet("/connect/authorize")]
        public IActionResult Authorize()
        {
            // 👉 log tất cả cookie
            foreach (var cookie in Request.Cookies)
            {
                Console.WriteLine($"[COOKIE] {cookie.Key} = {cookie.Value}");
            }


            // 👉 chưa login → redirect login
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge(
                    authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.GetEncodedUrl()
                    });
            }

            // 👉 đã login → cấp token
            var identity = new ClaimsIdentity(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // 👉 copy claims từ cookie sang token
            identity.AddClaim(OpenIddictConstants.Claims.Subject,
                User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value);

            identity.AddClaim(OpenIddictConstants.Claims.Name,
                User.Identity.Name);

            var principal = new ClaimsPrincipal(identity);

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpPost("/account/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _userManager.FindByNameAsync(req.Username);

            if (user == null || !await _userManager.CheckPasswordAsync(user, req.Password))
                return Unauthorized();

            // tạo identity
            var identity = new ClaimsIdentity(
                CookieAuthenticationDefaults.AuthenticationScheme);

            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.Id));
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Name, user.UserName));

            var principal = new ClaimsPrincipal(identity);

            // 👉 login bằng cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);


            return Ok();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                Console.WriteLine("User is logging out: " + User.Identity.Name);
            }
            else
            {
                Console.WriteLine("Logout called but user not logged in");
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect("http://localhost:3001");
        }
    }
}
