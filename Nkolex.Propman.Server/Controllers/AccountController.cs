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
        [HttpPost]

        public async Task<IActionResult> Register([FromBody] CreateAccountRequest request)
        {
            ICreateAccountResponse response = new CreateAccountResponse
            {
                Success = true,
                Message = "Account created successfully",
                UserId = "generated-user-id"
            };

            return Ok(response);
        }
    }
}
