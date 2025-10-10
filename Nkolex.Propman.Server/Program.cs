using Microsoft.OpenApi.Models;
using Nkolex.Propman.Server.Abstractions;
using Nkolex.Propman.Server.Data;
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
            builder.Services.AddControllers();

            // Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontendClients",
                    policy =>
                    {
                        policy.WithOrigins(
                            "http://localhost:4200",
                            "http://143.110.171.111:5001"
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

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
    public partial class Program { }
}
