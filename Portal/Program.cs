//JTorralba
using Microsoft.AspNetCore.Components.Authorization;
using System.Reflection;
using System.Text;

//JTorralba
using Blazored.SessionStorage;
using Radzen;
using Serilog;

//JTorralba
using Standard.Databases;
using Portal.Services;
using Standard.Functions;

namespace Portal
{
    public class Program
    {
        //JTorralba
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public static void Main(string[] args)
        {
            //JTorralba
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Log.Logger = Core.CreateLog();
            Console.Title = $"{Configuration["Title"]} (Portal)";
            Console.OutputEncoding = Encoding.UTF8;

            var builder = WebApplication.CreateBuilder(args);

            //JTorralba
            builder.Logging.ClearProviders();
            builder.Host.UseSerilog();

            // Add services to the container.
            builder.Services.AddRazorPages();

            //JTorralba
            builder.Services.AddServerSideBlazor()
                .AddHubOptions(options =>
                {
                    options.ClientTimeoutInterval = TimeSpan.FromSeconds(34);
                    options.EnableDetailedErrors = true;
                    options.HandshakeTimeout = TimeSpan.FromSeconds(17);
                    options.KeepAliveInterval = TimeSpan.FromSeconds(17);
                    options.MaximumParallelInvocationsPerClient = 1;
                    options.MaximumReceiveMessageSize = null;
                    options.StreamBufferCapacity = 17;
                })
                .AddCircuitOptions(options =>
                {
                    options.DetailedErrors = true;
                    options.DisconnectedCircuitMaxRetained = 1024;
                    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(17);
                    options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(68);
                    options.MaxBufferedUnacknowledgedRenderBatches = 17;
                }
                );

            //JTorralba
            builder.Services.AddRadzenComponents();
            builder.Services.AddBlazoredSessionStorage();
            builder.Services.AddSingleton<StateService>();
            builder.Services.AddScoped<AuthenticationStateProvider, AuthenticationStateService>();
            builder.Services.AddScoped<AuthenticationStateService>();
            builder.Services.AddSingleton<DBSingleton>();
            builder.Services.AddSingleton<DBSingletonService>();
            builder.Services.AddScoped<DBScoped>();
            builder.Services.AddScoped<DBScopedService>();
            builder.Services.AddScoped<ChatService>();
            builder.Services.AddScoped<LoginService>();
            builder.Services.AddScoped<TranscriptService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            //JTorralba
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}
