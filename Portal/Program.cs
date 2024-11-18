//JTorralba
using Microsoft.AspNetCore.Components.Authorization;
using System.Text;

//JTorralba
using Blazored.SessionStorage;
using Radzen;

//JTorralba
using Portal.Services;

namespace Portal
{
    public class Program
    {
        //JTorralba
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public static void Main(string[] args)
        {
            //JTorralba
            Console.Title = $"{Configuration["Title"]} (Portal)";
            Console.OutputEncoding = Encoding.UTF8;

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            //JTorralba
            builder.Services.AddRadzenComponents();
            builder.Services.AddScoped<AuthenticationStateProvider, AuthenticationStateService>();
            builder.Services.AddBlazoredSessionStorage();
            builder.Services.AddScoped<ChatService>();
            builder.Services.AddScoped<TranscriptService>();
            builder.Services.AddScoped<LoginService>();
            builder.Services.AddSingleton<StateService>();

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
