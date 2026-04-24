using Castle.Core.Logging;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nkolex.Propman.Server;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Models.DTOs;
using Nkolex.Propman.Server.Services;
using NSubstitute;
using NSubstitute.Extensions;

namespace Nkolex.Propman.Tests
{
    [Collection("One at the time fellows")]

    public class AuthServiceTests : TestFixture
    {
        private readonly IAuthService _authService;
        private IAccountDataService<IAccount> _accountDataService;

        public AuthServiceTests() : base(new TestWebApplicationFactory<Program>())
        {
            _authService = TestScope.ServiceProvider.GetRequiredService<IAuthService>(); ;
            _accountDataService = Factory.Services.GetRequiredService<IAccountDataService<IAccount>>();
        }

        [Fact]
        public async Task ValidateUserAsync_Given_Null_User_Should_Throw_ArgumentNullException()
        {
            User? user = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => _authService.ValidateUserAsync(user!, []));
        }

        [Fact]
        public async Task ValidateUserAsync_Given_Null_Users_Should_Throw_ArgumentNullException()
        {
            var user = CreateUser();
            List<User>? users = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => _authService.ValidateUserAsync(user, users!));
        }

        [Fact]
        public async Task Given_Valid_User_ValidateUserAsync_Should_Get_All_Users_Find_User_By_Email()
        {
            var user = CreateUser();

            _accountDataService = Substitute.For<IAccountDataService<IAccount>>();
            _accountDataService.GetAllAsync().Returns(CreateAccountList());

            var logger = Substitute.For<ILogger<AuthService>>();
            var configuration = Substitute.For<Microsoft.Extensions.Configuration.IConfiguration>();

            var users = new List<User>();
            var authService = new AuthService(logger,_accountDataService, configuration);
            var result = await authService.ValidateUserAsync(user, users);

            Assert.NotNull(result);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.PasswordHash, result.PasswordHash);
        }

        [Fact]
        public async Task Given_Wrong_Password_ValidateUserAsync_Should_Return_UnAuthorised()
        {
            var user = CreateUser();
            _accountDataService = Substitute.For<IAccountDataService<IAccount>>();
            _accountDataService.GetAllAsync().Returns(CreateAccountList());

            var logger = Substitute.For<ILogger<AuthService>>();
            var configuration = Substitute.For<Microsoft.Extensions.Configuration.IConfiguration>();

            var users = new List<User>();
            var authService = new AuthService(logger, _accountDataService, configuration);
            user.PasswordHash = "PasswordChanged";

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.ValidateUserAsync(user, users));
        }

        [Fact]
        public async Task Given_Invalid_User_GenerateJwtAsync_Return_Exception()
        {
            User? user = null;
            await Assert.ThrowsAsync<ArgumentNullException>(() => _authService.GenerateJwtAsync(user!));
        }

        [Fact]
        public async Task Given_Valid_User_GenerateJwtAsync_Should_Return_Token()
        {
            var user = CreateUser();
            var logger = Substitute.For<ILogger<AuthService>>();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(FakeJwtConfig())
                .Build();

            var authService = new AuthService(logger, _accountDataService, configuration);

            var result = await authService.GenerateJwtAsync(user);

            Assert.NotNull(result);
            Assert.IsType<string>(result);
        }

        [Fact]
        public async Task Given_Valid_Email_GetUserByID_Should_Return_One_User()
        {
            var user = CreateUser();
            
            var logger = Substitute.For<ILogger<AuthService>>();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(FakeJwtConfig())
                .Build();
            var accountDataService = Substitute.For<IAccountDataService<IAccount>>();
            accountDataService.GetAllAsync()
                .Returns([new Account
                {
                    Email = user.Email,
                    Name = "Joe",
                    Surname = "Dohn",
                    Password = "Password123!"
                }]);


            var authService = new AuthService(logger, accountDataService, configuration);
            var sud = await authService.GetUserByIdAsync(user.Email);

            Assert.NotNull(sud);
            Assert.Equal(user.Email, sud.Email);
            Assert.Equal(user.FullName, sud.FullName);
            Assert.Equal(user.PasswordHash, sud.PasswordHash);
        }

        private static User CreateUser()
        {
            var user = new User
            {
                Email = "test@example.com",
                PasswordHash = "Password123!",
                FullName = "Joe Dohn"
            };
            return user;
        }

        private static List<IAccount> CreateAccountList()
        {
            return new List<IAccount>
            {
                new Account { Email = "test@example.com", Password = "Password123!" },
                new Account { Email = "test1@example.com", Password = "Password123!" },
                new Account { Email = "test2@example.com", Password = "Password123!" }
            };
        }

        private static Dictionary<string, string?> FakeJwtConfig()
        {
            return new Dictionary<string, string?>
            {
                { "Jwt:Key", "sxOd205hHeW7Zl9NwMxsLTb1e9pqXnq6APhoQW7zM2o" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:ExpireMinutes", "60" }
            };
        }

        private static List<User> CreateUserList()
        {
            return new List<User>
            {
                new User { Email = "test@example.com", PasswordHash = "Password123!" },
                new User { Email = "test1@example.com", PasswordHash = "Password123!" },
                new User { Email = "test2@example.com", PasswordHash = "Password123!" }
            };
        }

        private static List<User> AccountToUser(List<IAccount> accounts)
        {
            var users = new List<User>();
            foreach (var account in accounts)
            {
                var user = new User
                {
                    Email = account.Email,
                    PasswordHash = account.Password
                };
                users.Add(user);
            }
            return users;
        }
    }
}
