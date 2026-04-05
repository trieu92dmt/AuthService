using Authentication.DTOs;
using Authentication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;

        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost]
        public IActionResult LoginJWT([FromBody] LoginRequest request)
        {
            if (request.UserName == "admin" && request.Password == "password")
            {
                var token = _jwtService.GenerateToken(request.UserName);
                return Ok(new { token });
            }
            else
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }
        }

        [HttpPost("exchange")]
        public async Task<IActionResult> Exchange([FromBody] ExchangeRequest req)
        {
            var client = new HttpClient();

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = req.Code,
                ["redirect_uri"] = "http://localhost:3001/callback",
                ["client_id"] = "resource-client",
                ["client_secret"] = "secret"
            };

            var response = await client.PostAsync(
                "http://localhost:5000/connect/token",
                new FormUrlEncodedContent(form)
            );

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
    }
}
