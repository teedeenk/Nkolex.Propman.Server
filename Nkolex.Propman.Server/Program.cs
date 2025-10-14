using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Data;
using Nkolex.Propman.Server.Data.ConnectionOptions;
using Nkolex.Propman.Server.Data.Repositories;
using Nkolex.Propman.Server.Models;
using Nkolex.Propman.Server.Models.DTOs;
using Nkolex.Propman.Server.Services;

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

            // Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontendClients",
                    policy =>
                    {
                        policy.WithOrigins(
                            "http://localhost:4200",
                            "http://143.110.171.111"
                        )
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
