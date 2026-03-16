using Authentication.Infracstructures.CustomAttributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorization]
    public class CourseController : ControllerBase
    {
        [HttpGet]
        public IActionResult CheckAuthorization()
        {
            return Ok("Welcome");
        }
    }
}
