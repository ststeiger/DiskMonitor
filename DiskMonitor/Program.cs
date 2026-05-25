
namespace DiskMonitor;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


public class Program
{


    public static async System.Threading.Tasks.Task<int> Main(string[] args)
    {
        Microsoft.AspNetCore.Builder.WebApplicationBuilder builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

        builder.Services.ConfigureHttpJsonOptions(
            o =>
            {
                o.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            }
        );

        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        // Add services to the container.
        // builder.Services.AddRazorPages();


        // ── App Bootstrap ─────────────────────────────────────────────────────────────
        Microsoft.AspNetCore.Builder.WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.MapGet("/", () => Microsoft.AspNetCore.Http.Results.Content(DiskMonitorHtml.GetPage(), "text/html; charset=utf-8"));
        app.MapGet("/diskmonitor/api/disks", () => Microsoft.AspNetCore.Http.Results.Json(DiskService.GetDisks()));

        app.MapGet("/diskmonitor/js/d3.min.js", (Microsoft.AspNetCore.Http.HttpContext context) =>
        {
            System.IO.Stream? stream = EmbeddedResourceHelper.GetResourceStream("d3.min.js", typeof(Program));

            if (stream == null)
                return Microsoft.AspNetCore.Http.Results.NotFound();

            // Return the stream with the correct content type
            return Microsoft.AspNetCore.Http.Results.Stream(stream, "application/javascript");
        });

        // app.MapStaticAssets();
        // app.MapRazorPages().WithStaticAssets();

        await app.RunAsync();

        return 0;
    } // End Task Main 


} // End Class Program 
