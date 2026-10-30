using Microsoft.Extensions.DependencyInjection;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Tests
{
    [Collection("One at the time fellows")]
    public class ActivityServiceTests : TestFixture
    {
        private readonly IActivityService _activityService;
        private readonly IPropertyDataService<IProperty> _propertyDataService;
        public ActivityServiceTests() : base(new TestWebApplicationFactory<Program>())
        {
            _activityService = Factory.Services.GetRequiredService<IActivityService>();
            _propertyDataService = Factory.Services.GetRequiredService<IPropertyDataService<IProperty>>();
        }

        [Fact]
        public async Task Given_Activity_Should_Create_Activity()
        {
            var manager = Guid.NewGuid();
            var property = await CreatePropertyAsync(manager);
            var activity = CreateActivity(property.Id, manager);

            var sut = await _activityService.CreateActivityAsync(activity);
            Assert.NotNull(sut);
            Assert.Equal(activity.Id, sut.Id);
            Assert.Equal(activity.Name, sut.Name);
            Assert.Equal(activity.Description, sut.Description);
            Assert.Equal(manager, sut.CreatedBy);
            Assert.Equal(property.Id, sut.PropertyId);
        }

        [Fact]
        public async Task Given_Activity_Exists_GetById_As_PropertyManager_Should_Return_The_Activity()
        {
            var manager = Guid.NewGuid();
            var property = await CreatePropertyAsync(manager);
            var activity = CreateActivity(property.Id, manager);
            await _activityService.CreateActivityAsync(activity);

            var sut = await _activityService.GetByIdAsync(activity.Id, manager, isAdmin: false);
            Assert.NotNull(sut);
            Assert.Equal(activity.Id, sut.Id);
        }

        [Fact]
        public async Task Given_Activity_Exists_GetById_As_Tenant_Of_Another_Property_Should_Throw()
        {
            var manager = Guid.NewGuid();
            var property = await CreatePropertyAsync(manager);
            var activity = CreateActivity(property.Id, manager);
            await _activityService.CreateActivityAsync(activity);

            var otherTenant = Guid.NewGuid();
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _activityService.GetByIdAsync(activity.Id, otherTenant, isAdmin: false));
        }

        [Fact]
        public async Task Given_GetActivitiesForPropertyAsync_As_Tenant_Should_Return_Only_That_Propertys_Activities()
        {
            var manager1 = Guid.NewGuid();
            var property1 = await CreatePropertyAsync(manager1);
            var tenant1 = property1.Tenants[0];
            var activity1 = CreateActivity(property1.Id, manager1);
            var activity2 = CreateActivity(property1.Id, manager1);
            await _activityService.CreateActivityAsync(activity1);
            await _activityService.CreateActivityAsync(activity2);

            var manager2 = Guid.NewGuid();
            var property2 = await CreatePropertyAsync(manager2);
            var activity3 = CreateActivity(property2.Id, manager2);
            await _activityService.CreateActivityAsync(activity3);

            var result = await _activityService.GetActivitiesForPropertyAsync(property1.Id, tenant1, isAdmin: false);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, a => Assert.Equal(property1.Id, a.PropertyId));
        }

        [Fact]
        public async Task Given_GetActivitiesForPropertyAsync_As_NonMember_Should_Throw()
        {
            var manager = Guid.NewGuid();
            var property = await CreatePropertyAsync(manager);
            var activity = CreateActivity(property.Id, manager);
            await _activityService.CreateActivityAsync(activity);

            var outsider = Guid.NewGuid();
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _activityService.GetActivitiesForPropertyAsync(property.Id, outsider, isAdmin: false));
        }

        [Fact]
        public async Task Given_Activity_UpdateActivityAsync_Should_Update_Activity()
        {
            var manager = Guid.NewGuid();
            var property = await CreatePropertyAsync(manager);
            var activity = CreateActivity(property.Id, manager);
            await _activityService.CreateActivityAsync(activity);

            activity.Done = true;
            var sut = await _activityService.UpdateActivityAsync(activity);
            Assert.True(sut.Done);

            var getUpdated = await _activityService.GetByIdAsync(activity.Id, manager, isAdmin: false);
            Assert.True(getUpdated.Done);
        }

        [Fact]
        public async Task Given_Activity_DeleteActivityAsync_Should_Soft_Delete_Activity()
        {
            var manager = Guid.NewGuid();
            var property = await CreatePropertyAsync(manager);
            var activity = CreateActivity(property.Id, manager);
            await _activityService.CreateActivityAsync(activity);

            var result = await _activityService.DeleteActivityAsync(activity.Id);
            Assert.Equal(1, result);

            var all = await _activityService.GetActivitiesForPropertyAsync(property.Id, manager, isAdmin: false);
            Assert.DoesNotContain(all, a => a.Id == activity.Id);
        }

        private async Task<IProperty> CreatePropertyAsync(Guid manager)
        {
            var property = Factory.Services.GetRequiredService<IProperty>();
            property.Id = Guid.NewGuid();
            property.Name = "PropertyName";
            property.Address = "SomeWhereFun";
            property.PropertyManager = manager;
            property.Tenants = [Guid.NewGuid()];
            await _propertyDataService.AddAsync(property);
            return property;
        }

        private IActivity CreateActivity(Guid propertyId, Guid createdBy)
        {
            var activity = Factory.Services.GetRequiredService<IActivity>();
            activity.Id = Guid.NewGuid();
            activity.Name = "Fire Drill";
            activity.Description = "Annual fire drill";
            activity.Date = DateOnly.FromDateTime(DateTime.UtcNow);
            activity.Time = new TimeOnly(9, 0);
            activity.Draft = false;
            activity.Active = true;
            activity.Done = false;
            activity.IsDeleted = false;
            activity.PropertyId = propertyId;
            activity.CreatedBy = createdBy;
            return activity;
        }
    }
}
