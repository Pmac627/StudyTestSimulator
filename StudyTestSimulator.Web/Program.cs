using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.EntityFrameworkCore;
using StudyTestSimulator.Web.Data;
using StudyTestSimulator.Web.Services;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add authentication with Microsoft Entra ID
        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

        builder.Services.AddAuthorization();
        builder.Services.AddCascadingAuthenticationState();

        // Add anti-forgery service
        builder.Services.AddAntiforgery();

        // Add services to the container
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor()
            .AddMicrosoftIdentityConsentHandler();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Add database context
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Register application services
        builder.Services.AddScoped<IQuestionService, QuestionService>();
        builder.Services.AddScoped<ITestService, TestService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();

        // Add controllers with views for authentication endpoints
        builder.Services.AddControllersWithViews()
            .AddMicrosoftIdentityUI();

        var app = builder.Build();

        // Configure the HTTP request pipeline
        // Trust IIS forwarded headers so HTTPS scheme is preserved for auth redirects.
        // Clear KnownNetworks/KnownProxies so headers are trusted from any source
        // (required on shared hosting where IIS may not be on loopback).
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };
        
        forwardedHeadersOptions.KnownIPNetworks.Clear();
        forwardedHeadersOptions.KnownProxies.Clear();

        app.UseForwardedHeaders(forwardedHeadersOptions);

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAntiforgery();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorComponents<StudyTestSimulator.Web.Components.App>()
            .AddInteractiveServerRenderMode();

        app.MapControllers();

        app.Run();
    }
}