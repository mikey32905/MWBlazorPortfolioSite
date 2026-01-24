using AeonRegistryAPI.Endpoints.Artifact;
using AeonRegistryAPI.Endpoints.CustomIdentityEndpoints;
using AeonRegistryAPI.Endpoints.Home;
using AeonRegistryAPI.Endpoints.Sites;
using AeonRegistryAPI.Services;
using AeonRegistryAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

///Build Section of API
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddCustomSwagger();

//get a connection to the database
var connectionString = DataUtility.GetConnectionString(builder.Configuration);

//configure for database context for PostgresSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

//add in Identity endpoints
builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options =>{
    options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

//Admin Policy
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

//Email Sender Service
builder.Services.AddTransient<IEmailSender, ConsoleEmailService>();

//custom services
builder.Services.AddScoped<ISiteService, SiteService>();
builder.Services.AddScoped<IArtifactMediaFileService, ArtifactMediaFileService>();
builder.Services.AddScoped<IArtifactService, ArtifactService>();

//enable validation for minimal APIs
builder.Services.AddValidation();

///App Section of API
var app = builder.Build();

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    await DataSeed.ManageDataAsync(scope.ServiceProvider);
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<BlockIdentityEndpoints>();

var authRouteGroup = app.MapGroup("/api/auth")
    .WithTags("Admin");

authRouteGroup.MapIdentityApi<ApplicationUser>();

app.MapGet("/", () => Results.Redirect("/index.html"));

app.MapCustomIdentityEndpoints();
app.MapSiteEndpoints();
app.MapArtifactMediaFileEndpoints();
app.MapArtifactEndpoints();
app.MapHomeEndpoints();

app.Run();

