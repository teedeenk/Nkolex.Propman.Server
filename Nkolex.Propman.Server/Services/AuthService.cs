using Microsoft.IdentityModel.Tokens;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;

namespace Nkolex.Propman.Server.Services
{
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IAccountDataService<IAccount>? _accountDataService;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher _passwordHasher;

        public AuthService(ILogger<AuthService> logger, IAccountDataService<IAccount> accountDataService, IConfiguration configuration, IPasswordHasher passwordHasher)
        {
            _logger = logger;
            _accountDataService = accountDataService ?? throw new ArgumentNullException(nameof(accountDataService));
            _configuration = configuration;
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task<User?> ValidateUserAsync(User user, List<User> users)
        {
            if (user is null)
            {
                _logger.LogWarning("ValidateUserAsync called with null user.");
                throw new ArgumentNullException(nameof(user));
            }

            if (users is null)
            {
                _logger.LogInformation("No users found in the system.");
                throw new ArgumentNullException(nameof(users));
            }

            users = await GetUsersAsync();
            var userFromList = users.FirstOrDefault(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase) && _passwordHasher.VerifyPassword(u.PasswordHash, user.PasswordHash));
            if (userFromList == null)
            {
                _logger.LogInformation("User is not authorised.");
                throw new UnauthorizedAccessException();
            }
            return userFromList;
        }
        public async Task<List<User>> GetUsersAsync()
        {
            if(_accountDataService is null )
            {
                _logger.LogWarning("{IAccountDataService} doesn't exist...", nameof(IAccountDataService<IAccount>));
                return [];
            }
            var usersFromStore = await _accountDataService.GetAllAsync();
            var users = ConvertAccountToUser(usersFromStore);

            return users;
        }

        private static List<User> ConvertAccountToUser(List<IAccount> accounts)
        {
            var users = new List<User>();
            foreach(var account in accounts)
            {
                var user = new User
                {
                    Id = account.Id,
                    Email = account.Email,
                    PasswordHash = account.Password,
                    FullName = $"{account.Name} {account.Surname}",
                    Roles = account.Roles
                };
                users.Add(user);
            }
            return users;
        }
        public async Task<string> GenerateJwtAsync(User user)
        {
            if (user is null)
            {
                _logger.LogWarning("ValidateUserAsync called with null user.");
                throw new ArgumentNullException(nameof(user));
            }

            if (_configuration is null)
            {
                _logger.LogWarning("Configuration is null.");
                throw new ArgumentNullException(nameof(_configuration));
            }

            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                _logger.LogWarning("Jwt:Key is missing in configuration.");
                throw new InvalidOperationException("Jwt:Key is missing in configuration.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            if(credentials is null)
            {
                _logger.LogInformation("Credentials are missing in config");
                throw new InvalidOperationException("Credentials are missing in config");
            }

            var claims = new List<Claim>()
            {
                new(ClaimTypes.Email, user.Email),
                new(JwtRegisteredClaimNames.Sub, user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            var result = new JwtSecurityTokenHandler().WriteToken(token);
            return await Task.FromResult(result);
        }

        public async Task<User> GetUserByIdAsync(string email)
        {
            if (_accountDataService is null)
            {
                _logger.LogWarning("{IAccountDataService} doesn't exist...", nameof(IAccountDataService<IAccount>));
                throw new ArgumentNullException(nameof(email));
            }
            var allusers = await _accountDataService.GetAllAsync();
            if (allusers.Count > 0)
            {
                foreach (var account in allusers)
                {
                    if (account.Email.Equals(email))
                    {
                        var user = new User
                        {
                            Email = account.Email,
                            FullName = $"{account.Name} {account.Surname}",
                            PasswordHash = account.Password,
                            Roles = account.Roles,
                            SubscriptionTier = account.SubscriptionTier
                        };
                        return user;
                    }
                }
            }
            throw new KeyNotFoundException($"User with email '{email}' not found.");
        }
    }
}