using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToDoListAppA2.Data;
using ToDoListAppA2.Models;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);
 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); //Default connection

builder.Services.AddDefaultIdentity<ApplicationUser>(options => //Identity
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>() //Identity Roles
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews(); //Views

//builder.Services.ConfigureApplicationCookie(options => 
//{
//    options.LoginPath = "/Identity/Account/Login";
//});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.SlidingExpiration = false;
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
});

var app = builder.Build(); //build

if (!app.Environment.IsDevelopment()) //Error page
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

//app.Use(async (context, next) =>
//{
//    //Fix for not signing out user when starting application
//    if (context.User.Identity.IsAuthenticated && !context.Request.Path.StartsWithSegments("/Identity/Account/Login"))
//    {
//        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
//    }
//    await next();
//});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

using (var scope = app.Services.CreateScope()) //Ensures Roles Exist
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Normal", "Premium", "Admin", "Disabled" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

app.Run();
