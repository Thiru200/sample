using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(); // Add cookie authentication or configure other schemes

builder.Services.AddAuthorization(); // Add any specific authorization policies if needed

// Add controllers
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure middleware
app.UseStaticFiles(); // Serves static files like CSS, JS, images
app.UseRouting(); // Adds routing capabilities

app.UseAuthentication(); // Authenticates users
app.UseAuthorization(); // Authorizes users

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
