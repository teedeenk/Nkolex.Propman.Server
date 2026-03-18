using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Constants;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Models.DTOs;

namespace Nkolex.Propman.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] CreateAccountRequest request)
        {
            var response = await _accountService.AddUserAsync(request);

            return Ok(response);
        }

        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.PropertyManager}")]
        [HttpPut("approve")]
        public async Task<IActionResult> ApproveUser([FromBody] Account account)
        {
            try
            {
                await _accountService.ApproveUser(account);
                return Ok();
            }
            catch
            {
                return StatusCode(401, new { message = "User approval failed, please try again." });
            }
        }
    }
}
