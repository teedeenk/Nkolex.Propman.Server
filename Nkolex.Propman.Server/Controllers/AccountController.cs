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

            if (!response.Success)
            {
                return Conflict(response);
            }

            return Ok(response);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            var confirmed = await _accountService.ConfirmEmailAsync(token);
            if (!confirmed)
            {
                return BadRequest(new { message = "Invalid or expired email confirmation token." });
            }
            return Ok(new { message = "Email confirmed successfully." });
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationRequest request)
        {
            await _accountService.ResendConfirmationEmailAsync(request.Email);

            return Ok(new { message = "If an account with that email exists and isn't confirmed yet, a new confirmation email has been sent." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _accountService.ForgotPasswordAsync(request.Email);

            return Ok(new { message = "If an account with that email exists, a password reset email has been sent." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token) ||
                string.IsNullOrWhiteSpace(request.NewPassword) ||
                request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest(new { message = "Invalid request. Token and matching passwords are required." });
            }

            var result = await _accountService.ResetPasswordAsync(request.Token, request.NewPassword);
            if (!result)
            {
                return BadRequest(new { message = "Invalid or expired password reset token." });
            }

            return Ok(new { message = "Password reset successfully." });
        }

        public record ResendConfirmationRequest(string Email);
        public record ForgotPasswordRequest(string Email);
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

        [Authorize(Roles = $"{UserRoles.Admin}")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] Account account)
        {
            try
            {
                var result = await _accountService.UpdateUserAsync(account);
                if (!result)
                {
                    return NotFound(new { message = "User not found." });
                }
                return Ok();
            }
            catch (ArgumentException)
            {
                return BadRequest(new { message = "Invalid account data." });
            }
        }

        [Authorize(Roles = $"{UserRoles.Admin}")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var accounts = await _accountService.GetAllUsersAsync();
            return Ok(accounts);
        }

        [Authorize(Roles = $"{UserRoles.Admin}")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser([FromBody] Account account)
        {
            try
            {
                if (account == null || account.Id == Guid.Empty)
                {
                    return BadRequest(new { message = "Invalid account data. Account ID is required." });
                }

                var result = await _accountService.DeleteUserAsync(account);
                if (!result)
                {
                    return NotFound(new { message = "User not found." });
                }
                return Ok(new { message = "User deleted successfully." });
            }
            catch (ArgumentException)
            {
                return BadRequest(new { message = "Invalid account data." });
            }
        }
    }
}
