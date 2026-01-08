using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MWBlazorPortfolioSite;
using MWBlazorPortfolioSite.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<TerminalService>();
builder.Services.AddScoped<ProjectStateService>();
builder.Services.AddScoped<PortfolioImageService>();
builder.Services.AddScoped<ProjectManifestService>();

await builder.Build().RunAsync();
