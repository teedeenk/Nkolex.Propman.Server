using Microsoft.Extensions.DependencyInjection;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Tests
{
    [Collection("One at the time fellows")]
    public class ActivityDataServiceTests : TestFixture
    {
        private readonly IActivityDataService<IActivity> _activityDataService;
        private readonly IRepository<IActivity> _repo;
        public ActivityDataServiceTests() : base(new TestWebApplicationFactory<Program>())
        {
            _activityDataService = Factory.Services.GetRequiredService<IActivityDataService<IActivity>>();
            _repo = Factory.Services.GetRequiredService<IRepository<IActivity>>();
        }

        [Fact]
        public async Task Given_CreateActivity_AddAsync_Should_Return_1()
        {
            var activity = CreateActivity();
            var sut = await _activityDataService.AddAsync(activity);
            Assert.Equal(1, sut);
        }

        [Fact]
        public async Task Given_CreateActivity_AddAsync_should_Add_Activity()
        {
            var activity = CreateActivity();
            var sut = await _activityDataService.AddAsync(activity);
            Assert.Equal(1, sut);

            var repo = await _repo.GetByIdAsync(activity.Id);
            Assert.Equal(activity.Id, repo.Id);
            Assert.Equal(activity.Name, repo.Name);
            Assert.Equal(activity.Description, repo.Description);
            Assert.Equal(activity.Date, repo.Date);
            Assert.Equal(activity.Time, repo.Time);
        }

        [Fact]
        public async Task Given_ExistingActivity_UpdateAsync_Should_Update_Activity()
        {
            var activity = CreateActivity();
            await _activityDataService.AddAsync(activity);

            activity.Done = true;
            var sut = await _activityDataService.UpdateAsync(activity);
            Assert.Equal(1, sut);

            var repo = await _repo.GetByIdAsync(activity.Id);
            Assert.True(repo.Done);
        }

        [Fact]
        public async Task Given_ExistingActivity_GetByIdAsync_Should_Return_Activity()
        {
            var activity = CreateActivity();
            await _activityDataService.AddAsync(activity);

            var sut = await _activityDataService.GetByIdAsync(activity.Id);
            Assert.NotNull(sut);
            Assert.Equal(activity.Id, sut.Id);
        }

        [Fact]
        public async Task Given_Activities_GetAllAsync_Should_Return_All_Activities()
        {
            var activity1 = CreateActivity();
            var activity2 = CreateActivity();
            await _activityDataService.AddAsync(activity1);
            await _activityDataService.AddAsync(activity2);

            var all = await _activityDataService.GetAllAsync();
            Assert.True(all.Count >= 2);
        }

        private IActivity CreateActivity()
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
            activity.PropertyId = Guid.NewGuid();
            activity.CreatedBy = Guid.NewGuid();
            return activity;
        }
    }
}
