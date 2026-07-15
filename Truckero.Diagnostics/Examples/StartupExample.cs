using System;
using Microsoft.Extensions.DependencyInjection;
using Truckero.Diagnostics.Mocks.Services;

namespace Truckero.SampleUsage
{
    /// <summary>
    /// Example showing how to configure the FreightBidSimulator in an ASP.NET Core application
    /// </summary>
    public static class StartupExample
    {
        /// <summary>
        /// Example code showing how to register the FreightBidSimulator in Program.cs
        /// Note: This is not meant to be executed directly, it's just a reference
        /// </summary>
        public static void ConfigurationExample()
        {
            /*
            // In your Program.cs file:

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();
            
            // Add FreightBidSimulator in development only
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddFreightBidSimulator();
            }

            // Configure HTTP client for the simulator API
            builder.Services.AddHttpClient("FreightBidSimulator", client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001/");
            });

            // Add Swagger for API documentation
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
            */
        }
    }
}