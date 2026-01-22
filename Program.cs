using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using ScannerAndPicker.Services;

var builder = WebApplication.CreateBuilder(args);

// Add authentication with Microsoft Identity (sign-in only using implicit flow)
// Token acquisition for SharePoint/Graph is handled client-side via MSAL.js
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.GetSection("AzureAd").Bind(options);
        options.ResponseType = "id_token";
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

// Register API logging service for tracking Graph/SharePoint API calls
builder.Services.AddSingleton<IApiLogService, ApiLogService>();

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
