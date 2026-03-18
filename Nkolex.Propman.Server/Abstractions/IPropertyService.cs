using Nkolex.Propman.Server.Models;

namespace Nkolex.Propman.Server.Abstractions
{
    public interface IPropertyService
    {
        Task<IProperty> UploadPropertyAsync(IProperty property);
        Task<IProperty> GetByIdAsync(IProperty property);
        Task<IProperty> UpdatePropertyAsync(IProperty property);
    }
}
