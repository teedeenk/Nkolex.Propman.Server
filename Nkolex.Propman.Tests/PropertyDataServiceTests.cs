using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nkolex.Propman.Tests
{
    [Collection("One at the time fellows")]
    public class PropertyDataServiceTests : TestFixture
    {
        private readonly IPropertyDataService<IProperty> _propertyDataService;
        private readonly IRepository<IProperty> _repo;
        public PropertyDataServiceTests() : base(new TestWebApplicationFactory<Program>())
        {
            _propertyDataService = Factory.Services.GetRequiredService<IPropertyDataService<IProperty>>();
            _repo = Factory.Services.GetRequiredService<IRepository<IProperty>>();
        }

        [Fact]
        public async Task Given_CreateProperty_AddAsync_Should_Return_1()
        {
            var property = CreateProperty();
            var sut = await _propertyDataService.AddAsync(property);
            Assert.Equal(1, sut);
        }

        [Fact]
        public async Task Given_CreateProperty_AddSync_should_Add_Property()
        {
            var property = CreateProperty();
            var sut = await _propertyDataService.AddAsync(property);
            Assert.Equal(1, sut);

            var repo = await _repo.GetByIdAsync(property);
            Assert.Equal(property.Id, repo.Id);
            Assert.Equal(property.Name, repo.Name);
            Assert.Equal(property.Address, repo.Address);
            Assert.Equal(property.Tenants, repo.Tenants);
            Assert.Equal(property.Statement, repo.Statement);
        }

        private IProperty CreateProperty()
        {
            var property = Factory.Services.GetRequiredService<IProperty>();
            property.Id = Guid.NewGuid();
            property.Name = "PropertyName";
            property.Address = "SomeWhereFun";
            property.PropertyManager = Guid.NewGuid();
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
