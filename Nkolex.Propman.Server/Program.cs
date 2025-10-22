using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Data;
using Nkolex.Propman.Server.Data.ConnectionOptions;
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

            // Add services to the container.
            builder.Services.AddTransient<ICreateAccountRequest, CreateAccountRequest>();
            builder.Services.AddTransient<ICreateAccountResponse, CreateAccountResponse>();
            builder.Services.AddTransient<IAccountService, AccountService>();
            builder.Services.AddTransient<IAccount, Account>();
            builder.Services.AddTransient<IRepository<IAccount>, FlatFileRepository>();
            builder.Services.AddTransient<IAccountDataService, AccountDataService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IDataStore, DataStore>();

            var repoSection = builder.Configuration.GetSection("RepositoryOptions");
            builder.Services.Configure<FlatFileOptions>(repoSection.GetSection("FlatFile"));
            var repoType = repoSection.GetValue<string>("Type");

            if (repoType is "FlatFile")
            {
                builder.Services.AddSingleton<IRepositoryOptions>(sp =>
                    sp.GetRequiredService<IOptions<FlatFileOptions>>().Value);
                builder.Services.AddSingleton<IRepository<IAccount>, FlatFileRepository>();
            }
            else
            {
                throw new InvalidOperationException("Unsupported repository type");
            }

            builder.Services.AddControllers();

            //Configure JWT Authentication
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
                            builder.Configuration["Jwt:Key"]!)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization();

            // Add CORS policy
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

            // Add OpenAPI/Swagger services
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nkolex Propman API", Version = "v1" });
            });

            var app = builder.Build();

            // Use CORS policy
            app.UseCors("AllowFrontendClients");

            // Configure the HTTP request pipeline.
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
