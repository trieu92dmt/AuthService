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
    }
}
