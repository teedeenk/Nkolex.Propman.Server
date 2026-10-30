using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Constants;
using Nkolex.Propman.Server.Models;
using System.Security.Claims;

namespace Nkolex.Propman.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _activityService;
        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
        }

        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.PropertyManager}")]
        [HttpPost("createactivity")]
        public async Task<IActionResult> CreateActivity([FromBody] Activity activity)
        {
            try
            {
                if (!TryGetAccountId(out var accountId))
                {
                    return Unauthorized();
                }
                activity.Id = Guid.NewGuid();
                activity.CreatedBy = accountId;
                var response = await _activityService.CreateActivityAsync(activity);
                return Ok(response);
            }
            catch
            {
                return StatusCode(400, new { message = "Activity creation failed, please try again." });
            }
        }

        [HttpGet("activities/{propertyId}")]
        public async Task<IActionResult> GetActivitiesForProperty(Guid propertyId)
        {
            try
            {
                if (!TryGetAccountId(out var accountId))
                {
                    return Unauthorized();
                }
                var response = await _activityService.GetActivitiesForPropertyAsync(propertyId, accountId, User.IsInRole(UserRoles.Admin));
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch
            {
                return StatusCode(400, new { message = "Failed to retrieve activities, please try again." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!TryGetAccountId(out var accountId))
                {
                    return Unauthorized();
                }
                var response = await _activityService.GetByIdAsync(id, accountId, User.IsInRole(UserRoles.Admin));
                if (response is null)
                {
                    return NotFound();
                }
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch
            {
                return StatusCode(400, new { message = "Failed to retrieve activity, please try again." });
            }
        }

        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.PropertyManager}")]
        [HttpPut("updateactivity")]
        public async Task<IActionResult> UpdateActivity([FromBody] Activity activity)
        {
            try
            {
                var response = await _activityService.UpdateActivityAsync(activity);
                return Ok(response);
            }
            catch
            {
                return StatusCode(400, new { message = "Activity update failed, please try again." });
            }
        }

        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.PropertyManager}")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(Guid id)
        {
            try
            {
                await _activityService.DeleteActivityAsync(id);
                return Ok();
            }
            catch
            {
                return StatusCode(400, new { message = "Activity deletion failed, please try again." });
            }
        }

        private bool TryGetAccountId(out Guid accountId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out accountId);
        }
    }
}
