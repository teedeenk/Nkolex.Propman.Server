namespace Nkolex.Propman.Server.Abstractions
{
    public interface IActivityService
    {
        Task<IActivity> CreateActivityAsync(IActivity activity);
        Task<IActivity> GetByIdAsync(Guid id, Guid requestingAccountId, bool isAdmin);
        Task<IActivity> UpdateActivityAsync(IActivity activity);
        Task<List<IActivity>> GetActivitiesForPropertyAsync(Guid propertyId, Guid requestingAccountId, bool isAdmin);
        Task<int> DeleteActivityAsync(Guid id);
    }
}
