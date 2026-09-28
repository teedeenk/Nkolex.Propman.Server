using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Models.DTOs;

namespace Nkolex.Propman.Server.Abstractions
{
    public interface IPropertyService
    {
        Task<PropertyResponse> UploadPropertyAsync(IProperty property);
        Task<PropertyResponse> GetByIdAsync(Guid id);
        Task<PropertyResponse> UpdatePropertyAsync(IProperty property);
        Task<List<PropertyResponse>> GetAllPropertiesAsync();
    }
}
