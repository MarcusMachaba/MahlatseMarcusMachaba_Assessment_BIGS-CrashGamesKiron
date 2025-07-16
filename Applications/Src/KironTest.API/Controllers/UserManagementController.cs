using Core.ApplicationModels.KironTestAPI;
using KironTest.API.ServiceHelpers;
using Microsoft.AspNetCore.Mvc;

namespace KironTest.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserManagementController : ControllerBase
    {
        private readonly IAuthService _authService;
        public UserManagementController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterNewUserRequest request)
        {
            var response = await _authService.RegisterNewUserAsync(request);
            if (response.usr == null)
            {
                return BadRequest(response.validationErrors);
            }

            return Ok(new { response.usr.Id, response.usr.UserName });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Core.ApplicationModels.KironTestAPI.LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            if (response == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok(response);
        }
    }
}
