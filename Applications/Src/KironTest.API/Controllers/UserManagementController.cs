using Core.ApplicationModels.KironTestAPI;
using KironTest.API.ServiceHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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

        /// <summary>
        /// Registers a new user with hashed password.
        /// </summary>
        /// <param name="request">Username and password</param>
        /// <returns>Returns user ID and username if successful</returns>
        /// <response code="200">User successfully registered</response>
        /// <response code="400">Validation failed or user already exists</response>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(string), 400)]
        public async Task<IActionResult> Register([FromBody] RegisterNewUserRequest request)
        {
            var response = await _authService.RegisterNewUserAsync(request);
            if (response.usr == null)
            {
                return BadRequest(response.validationErrors);
            }

            return Ok(new { response.usr.Id, response.usr.UserName });
        }

        /// <summary>
        /// Authenticates a user and returns a JWT access token.
        /// </summary>
        /// <param name="request">Username and password</param>
        /// <returns>JWT access token and expiration info</returns>
        /// <response code="200">Login successful, token returned</response>
        /// <response code="401">Invalid credentials</response>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(TokenResponse), 200)]
        [ProducesResponseType(typeof(HttpStatusCode), (int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> Login([FromBody] Core.ApplicationModels.KironTestAPI.LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            if (response == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            return Ok(response);
        }
    }
}
