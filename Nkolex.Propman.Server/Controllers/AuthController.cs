using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Constants;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Models.DTOs;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Nkolex.Propman.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.PropertyManager}, {UserRoles.Tenant}")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if(request is null)
                {
                    _logger.LogInformation("request is missing...");
                    return BadRequest();
                }
                var requestConvertedToUser = ConvertRequestToUser(request);
                var user = await _authService.ValidateUserAsync(requestConvertedToUser, []);

                if (user is null)
                {
                    return Unauthorized(new { message = "Invalid Username or Password" });
                }

                var token = _authService.GenerateJwtAsync(user);
                return Ok(new LoginResponse
                {
                    Token = token.Result,
                    Email = user.Email,
                    Expiration = DateTime.UtcNow.AddHours(2),
                    FullName = user.FullName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error! during login");
                return StatusCode(401, new {message = "login failed."});
            }
        }

        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.PropertyManager}")]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if(email == null)
            {
                return BadRequest();
            }

            var user = _authService.GetUserByIdAsync(email);
            var fullName = $"{user.Result.FullName}";
            return Ok(new { fullName });
        }

        private static User ConvertRequestToUser(LoginRequest request)
        {
            return new User()
            {
                Email = request.Email,
                PasswordHash = request.Password
            };
        }
    }
}
