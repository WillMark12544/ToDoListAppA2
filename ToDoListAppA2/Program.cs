using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToDoListAppA2.Data;
using ToDoListAppA2.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

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

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

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

using (var scope = app.Services.CreateScope()) //Creates Test Admin - Needs to Change at some point
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string adminEmail = "test1.test@gmail.com"; // Replace with the actual user's email
    string roleName = "Admin";

    // Create the role if it doesn't exist
    if (!await roleManager.RoleExistsAsync(roleName))
    {
        await roleManager.CreateAsync(new IdentityRole(roleName));
    }

    // Find the user
    var user = await userManager.FindByEmailAsync(adminEmail);
    if (user != null)
    {
        // Ensure the user is in the Admin role
        if (!(await userManager.IsInRoleAsync(user, roleName)))
        {
            await userManager.AddToRoleAsync(user, roleName);
            Console.WriteLine($"User {adminEmail} added to Admin role.");
        }
        else
        {
            Console.WriteLine($"User {adminEmail} is already in the Admin role.");
        }
    }
    else
    {
        Console.WriteLine($"User {adminEmail} not found.");
    }
}


app.Run();


