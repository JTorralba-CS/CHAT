//JTorralba
using Microsoft.AspNetCore.ResponseCompression;
using System.Text;

using Standard.Hubs;

namespace SignalR
{
    public class Program
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public static void Main(string[] args)
        {
            //JTorralba
            string Title = Configuration["Title"];

            Console.Title = $"{Title} (SignalR)";
            Console.OutputEncoding = Encoding.UTF8;

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            //JTorralba
            //builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerGen(opt => opt.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = Title,
                Description = $"{Title} (SignalR WebAPI)"
            }));

            //JTorralba
            builder.Services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
            });

            //JTorralba
            builder.Services.AddSignalR(options =>
            {
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(34);
                options.EnableDetailedErrors = true;
                options.HandshakeTimeout = TimeSpan.FromSeconds(17);
                options.KeepAliveInterval = TimeSpan.FromSeconds(17);
                options.MaximumParallelInvocationsPerClient = 1;
                options.MaximumReceiveMessageSize = 1024000;
                options.StatefulReconnectBufferSize = 1024000;
                options.StreamBufferCapacity = 17;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            //JTorralba
            app.MapHub<ChatHub>("/chathub");

            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = summaries[Random.Shared.Next(summaries.Length)]
                    })
                    .ToArray();
                return forecast;
            })
            .WithName("GetWeatherForecast")
            .WithOpenApi();

            app.Run();
        }
    }
}
