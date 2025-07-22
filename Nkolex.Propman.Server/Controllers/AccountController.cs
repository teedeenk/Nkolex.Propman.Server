using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models;

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
    }
}
