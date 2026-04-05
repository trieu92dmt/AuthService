using APIServer.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APIServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("exchange")]
        public async Task<IActionResult> Exchange([FromBody] ExchangeRequest req)
        {
            var client = new HttpClient();

            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = req.code,
                ["redirect_uri"] = "http://localhost:3001/callback",
                ["client_id"] = "react-client",
                //["client_secret"] = "secret",
                ["code_verifier"] = req.code_verifier // 👈 QUAN TRỌNG
            };

            var response = await client.PostAsync(
                "https://localhost:5000/connect/token",
                new FormUrlEncodedContent(form)
            );

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
    }
}
