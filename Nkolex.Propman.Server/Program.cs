using Microsoft.OpenApi.Models;

namespace Nkolex.Propman.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            // Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontendClients",
                    policy =>
                    {
                        policy.WithOrigins(
                            "http://localhost:4200",
                            "https://staging.d3q91dmmdbfhfv.amplifyapp.com"
                        )
                        .WithMethods("GET","POST","OPTIONS")
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
}
