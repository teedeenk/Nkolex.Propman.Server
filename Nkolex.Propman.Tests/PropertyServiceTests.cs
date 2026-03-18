using Microsoft.Extensions.DependencyInjection;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Tests
{
    [Collection("One at the time fellows")]
    public class PropertyServiceTests : TestFixture
    {
        private readonly IPropertyService _propertyService;
        public PropertyServiceTests() : base(new TestWebApplicationFactory<Program>())
        {
            _propertyService = Factory.Services.GetRequiredService<IPropertyService>();
        }

        [Fact]
        public async Task Given_Property_Should_Create_Property()
        {
            var property = CreateProperty();
            var sud = await _propertyService.UploadPropertyAsync(property);
            Assert.NotNull(sud);
            Assert.Equal(property, sud);
        }

        [Fact]
        public async Task Given_Statement_With_Valid_Id_should_Update_Property_with_Statement_id()
        {
            var property = CreatePropertyWithOutStatement();

            var addProperty = await _propertyService.UploadPropertyAsync(property);
            Assert.NotNull(addProperty);

            var updatedProperty = property;
            updatedProperty.Statement = Guid.NewGuid();
            var sud = await _propertyService.UpdatePropertyAsync(updatedProperty);
            Assert.NotNull(sud);

            var getUpdatedProperty = await _propertyService.GetByIdAsync(property);
            Assert.NotNull(getUpdatedProperty);
            Assert.Equal(getUpdatedProperty.Statement, updatedProperty.Statement);
        }

        [Fact]
        public async Task Given_Property_Exists_GetById_Should_Return_the_Property()
        {
            var property = CreatePropertyWithOutStatement();
            var addProperty = await _propertyService.UploadPropertyAsync(property);
            Assert.NotNull(addProperty);

            var sud = await _propertyService.GetByIdAsync(property);
            Assert.NotNull(sud);
            Assert.Equal(property.Id, sud.Id);
            Assert.Equal(property.Name, sud.Name);
            Assert.Equal(property.Address, sud.Address);
            Assert.Equal(property.PropertyManager, sud.PropertyManager);
        }
        private IProperty CreatePropertyWithOutStatement()
        {
            var property = Factory.Services.GetRequiredService<IProperty>();
            property.Id = Guid.NewGuid();
            property.Name = "PropertyName";
            property.Address = "SomeWhereFun";
            property.PropertyManager = "PropertyMangager";
            property.Tenants = [CreateAccount().Id];
            property.Statement = Guid.Empty;
            return property;
        }
        private IProperty CreateProperty()
        {
            var property = Factory.Services.GetRequiredService<IProperty>();
            property.Id = Guid.NewGuid();
            property.Name = "PropertyName";
            property.Address = "SomeWhereFun";
            property.PropertyManager = "PropertyMangager";
            property.Tenants = [CreateAccount().Id];
            property.Statement = CreateStatement().Id;
            return property;
        }

        private IAccount CreateAccount()
        {
            var account = Factory.Services.GetRequiredService<IAccount>();
            account.Id = Guid.NewGuid();
            account.Name = "John";
            account.Surname = "Doe";
            account.PhoneNumber = "1234567890";
            account.Email = "john.doe@example.com";
            account.Password = "TestPassword123!";
            account.AgreeToTerms = true;
            account.Roles = ["Guest"];
            return account;
        }
        private IStatement CreateStatement()
        {
            var statement = Factory.Services.GetRequiredService<IStatement>();
            statement.Id = Guid.NewGuid();
            statement.StatementLines = [new(DateTime.UtcNow, "description", 4)];
            return statement;
        }
    }
}
