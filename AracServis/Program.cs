using Microsoft.EntityFrameworkCore;
using AracServis.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages
builder.Services.AddRazorPages();

// Database
builder.Services.AddDbContext<ServisDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Cookie Authentication EKLENDÝ
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/AdminLogin";        // Login sayfasý
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Cookie süresi
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//BUNLAR ÖNEMLÝ
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
