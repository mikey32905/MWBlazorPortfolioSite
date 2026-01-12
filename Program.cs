using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MWBlazorPortfolioSite;
using MWBlazorPortfolioSite.Services;
using System.Text.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<TerminalService>();
builder.Services.AddScoped<ProjectStateService>();
builder.Services.AddScoped<PortfolioImageService>();
builder.Services.AddScoped<ProjectManifestService>();

builder.Services.AddScoped(sp =>
{
    var options = new JsonSerializerOptions
    {
        // This is the "magic" line
        PropertyNameCaseInsensitive = true
    };

    return new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
});

// This is the simplest way to force Blazor to interpret the hash as a route
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
