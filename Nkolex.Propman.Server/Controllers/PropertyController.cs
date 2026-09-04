using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Constants;
using Nkolex.Propman.Server.Models;

namespace Nkolex.Propman.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService _propertyService;
        public PropertyController(IPropertyService propertyService)
        {
            _propertyService = propertyService ?? throw new ArgumentNullException(nameof(propertyService));
        }

        [Authorize(Roles = $"{UserRoles.Admin}, {UserRoles.PropertyManager}")]
        [HttpPost("uploadproperty")]
        public async Task<IActionResult> UploadProperty([FromBody] Property property)
        {
            try
            {
                property.Id = Guid.NewGuid();
                var response = await _propertyService.UploadPropertyAsync(property);
                return Ok();
            }
            catch 
            {
                return StatusCode(400, new { message = "Property add failed, please try again." });
            }
        }

        [Authorize(Roles = $"{UserRoles.Admin}")]
        [HttpGet("properties")]
        public async Task<ActionResult> GetAllProperties()
        {
            try
            {
                var response = await _propertyService.GetAllPropertiesAsync();
                return Ok(response);
            }
            catch
            {
                return StatusCode(400, new { message = "Failed to retrieve properties, please try again." });
            }
        }
    }
}
