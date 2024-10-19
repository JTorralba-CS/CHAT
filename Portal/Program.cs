//JTorralba
using Radzen;
using Portal.Services;
using System.Text;

namespace Portal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //JTorralba
            Console.Title = "Portal";
            Console.OutputEncoding = Encoding.UTF8;

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();

            //JTorralba
            builder.Services.AddRadzenComponents();
            builder.Services.AddScoped<TranscriptService>();
            builder.Services.AddScoped<ChatService>();

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

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}
