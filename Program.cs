using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Bramki.Data;

var builder = WebApplication.CreateBuilder(args);

var isDev = builder.Environment.IsDevelopment();
var bypass = builder.Configuration.GetValue<bool>("Auth:Bypass");

// Windows (Negotiate) auth so localhost/Kestrel also authenticates
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AppUsers", policy =>
    {
        policy.RequireAuthenticatedUser();

        // In Development with Bypass=true, skip the role check
        if (!(isDev && bypass))
        {
            var sid = builder.Configuration["Auth:AllowedGroupSid"];
            if (!string.IsNullOrWhiteSpace(sid))
            {
                policy.RequireRole(sid);
            }
            else
            {
                var group = builder.Configuration["Auth:AllowedGroup"];
                if (!string.IsNullOrWhiteSpace(group))
                    policy.RequireRole(group);
            }
        }
    });

    // Require auth by default
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Razor Pages
builder.Services.AddRazorPages(options =>
{
    // Lock whole site
    if (!(isDev && bypass))
        options.Conventions.AuthorizeFolder("/", "AppUsers");
    else
        options.Conventions.AllowAnonymousToFolder("/");

    // Make the People page also respond at the site root "/"
    options.Conventions.AddPageRoute("/People/Index", "");
});

// EF Core (SQL Server)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// Static files (wwwroot)
app.UseStaticFiles();

// Routing + Auth
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map pages
app.MapRazorPages();

app.Run();
