using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityDataService<IActivity> _dataService;
        private readonly IPropertyDataService<IProperty> _propertyDataService;

        public ActivityService(IActivityDataService<IActivity> dataService, IPropertyDataService<IProperty> propertyDataService)
        {
            _dataService = dataService;
            _propertyDataService = propertyDataService;
        }

        public async Task<IActivity> CreateActivityAsync(IActivity activity)
        {
            ArgumentNullException.ThrowIfNull(activity);
            if (activity.PropertyId == Guid.Empty)
            {
                throw new ArgumentException("PropertyId must be a valid, non-empty Guid.", nameof(activity));
            }
            var property = await _propertyDataService.GetByIdAsync(activity.PropertyId);
            if (property is null)
            {
                throw new ArgumentException("Property does not exist.", nameof(activity));
            }
            await _dataService.AddAsync(activity);
            return activity;
        }

        public async Task<int> DeleteActivityAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Id must be a valid, non-empty Guid.", nameof(id));
            }
            var activity = await _dataService.GetByIdAsync(id);
            if (activity is null)
            {
                return 0;
            }
            activity.IsDeleted = true;
            return await _dataService.UpdateAsync(activity);
        }

        public async Task<List<IActivity>> GetActivitiesForPropertyAsync(Guid propertyId, Guid requestingAccountId, bool isAdmin)
        {
            if (propertyId == Guid.Empty)
            {
                throw new ArgumentException("PropertyId must be a valid, non-empty Guid.", nameof(propertyId));
            }

            if (!isAdmin)
            {
                await EnsureAccountBelongsToPropertyAsync(propertyId, requestingAccountId);
            }

            var activities = await _dataService.GetAllAsync();
            return activities
                .Where(a => !a.IsDeleted && a.PropertyId == propertyId)
                .ToList();
        }

        public async Task<IActivity> GetByIdAsync(Guid id, Guid requestingAccountId, bool isAdmin)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Id must be a valid, non-empty Guid.", nameof(id));
            }
            var activity = await _dataService.GetByIdAsync(id);
            if (activity is null)
            {
                return null!;
            }

            if (!isAdmin)
            {
                await EnsureAccountBelongsToPropertyAsync(activity.PropertyId, requestingAccountId);
            }

            return activity;
        }

        public async Task<IActivity> UpdateActivityAsync(IActivity activity)
        {
            ArgumentNullException.ThrowIfNull(activity);
            await _dataService.UpdateAsync(activity);
            return activity;
        }

        private async Task EnsureAccountBelongsToPropertyAsync(Guid propertyId, Guid accountId)
        {
            var property = await _propertyDataService.GetByIdAsync(propertyId);
            if (property is null)
            {
                throw new UnauthorizedAccessException("Property does not exist.");
            }

            var belongsToProperty = property.PropertyManager == accountId || property.Tenants.Contains(accountId);
            if (!belongsToProperty)
            {
                throw new UnauthorizedAccessException("You do not have access to this property's activities.");
            }
        }
    }
}
