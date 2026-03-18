using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Constants;
using Nkolex.Propman.Server.Data;
using Nkolex.Propman.Server.Data.ConnectionOptions;
using Nkolex.Propman.Server.Data.DataBaseConfig;
using Nkolex.Propman.Server.Data.Repositories;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Models.DTOs;
using Nkolex.Propman.Server.Services;
using System.Text;

namespace Nkolex.Propman.Server
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddTransient<ICreateAccountRequest, CreateAccountRequest>();
            builder.Services.AddTransient<ICreateAccountResponse, CreateAccountResponse>();
            builder.Services.AddTransient<IAccountService, AccountService>();
            builder.Services.AddTransient<IAccount, Account>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(FlatFileRepository<>));
            builder.Services.AddTransient<IAccountDataService<IAccount>, AccountDataService<IAccount>>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton(typeof(IDataStore<>), typeof(DataStore<>));
            builder.Services.AddTransient<IStatement, Statement>();
            builder.Services.AddTransient<IUploadCsvDataService<Statement, StatementLine>, UploadCsvDataService>();
            builder.Services.AddTransient<IUploadCsvService, UploadCsvService>();
            builder.Services.AddScoped<IProcessCsvFileService, ProcessCsvFileService>();
            builder.Services.AddTransient<IProperty, Property>();
            builder.Services.AddTransient<IPropertyService, PropertyService>();
            builder.Services.AddTransient<IPropertyDataService<IProperty>, PropertyDataService<IProperty>>();
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<Func<Type, Type?>>(sp =>
                t =>
                {
                    var impl = sp.GetService(t);
                    return impl?.GetType();
                }
            );
            builder.Services.AddHttpContextAccessor();

            var repoSection = builder.Configuration.GetSection("RepositoryOptions");
            builder.Services.Configure<FlatFileOptions>(repoSection.GetSection("FlatFile"));
            var repoType = repoSection.GetValue<string>("Type");

            if (repoType is "FlatFile")
            {
                builder.Services.AddSingleton<IRepositoryOptions>(sp =>
                    sp.GetRequiredService<IOptions<FlatFileOptions>>().Value);
                builder.Services.AddSingleton(typeof(IRepository<>),typeof(FlatFileRepository<>));
            }
            else
            {
                throw new InvalidOperationException("Unsupported repository type");
            }
            builder.Services.Configure<Tables>(builder.Configuration.GetSection("Tables"));
            builder.Services.AddSingleton<ITables>(sp => sp.GetRequiredService<IOptions<Tables>>().Value);

            builder.Services.AddControllers();

            var jwtSecretFile = "/etc/propmanserver/jwt-secret.txt";
            var jwtKey = string.Empty;
            if (File.Exists(jwtSecretFile))
            {
                jwtKey = File.ReadAllText(jwtSecretFile).Trim();
                builder.Configuration["Jwt:Key"] = jwtKey;
            }
            else
            {
                jwtKey = builder.Configuration["Jwt:Key"]!;
            }

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                            jwtKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("RequireAdminRole", policy => 
                    policy.RequireRole(UserRoles.Admin))
                .AddPolicy("RequireManagerOrAdmin", policy => 
                    policy.RequireRole(UserRoles.Admin, UserRoles.PropertyManager))
                .AddPolicy("CanManageProperties", policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRole(UserRoles.Admin) ||
                        context.User.IsInRole(UserRoles.PropertyManager)));

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontendClients",
                    policy =>
                    {
                        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins")
                        .Get<string[]>();
                        policy.WithOrigins(allowedOrigins!)
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    });
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nkolex Propman API", Version = "v1" });
            });

            var app = builder.Build();

            app.UseCors("AllowFrontendClients");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nkolex Propman API v1");
                });
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
    public partial class Program { }
}
