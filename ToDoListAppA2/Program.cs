using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToDoListAppA2.Data;
using ToDoListAppA2.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Stripe;
using NuGet.Packaging;
using ToDoListAppA2.DataAccess.Repository.IRepository;
using ToDoListAppA2.DataAccess.Repository;

var builder = WebApplication.CreateBuilder(args);
 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); //Default connection

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); // Register UnitOfWork - Manages changes to DB

builder.Services.AddDefaultIdentity<ApplicationUser>(options => //Identity
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>() //Identity Roles
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews(); //Views

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.SlidingExpiration = false;
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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

app.UseSession();

app.UseAuthentication();

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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string adminEmail = "test1.test@gmail.com";
    string adminPassword = "Admin123!"; 
    string roleName = "Admin";

    // Create the role if it doesn't exist
    if (!await roleManager.RoleExistsAsync(roleName))
    {
        await roleManager.CreateAsync(new IdentityRole(roleName));
    }

    // Create the user if they don't exist
    var user = await userManager.FindByEmailAsync(adminEmail);
    if (user == null)
    {
        user = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, adminPassword);
        if (!result.Succeeded)
        {
            Console.WriteLine($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            return;
        }

        Console.WriteLine($"Admin user {adminEmail} created.");
    }

    // 3. Add to Admin role
    if (!await userManager.IsInRoleAsync(user, roleName))
    {
        await userManager.AddToRoleAsync(user, roleName);
        Console.WriteLine($"Admin user {adminEmail} added to Admin role.");
    }
    else
    {
        Console.WriteLine($"Admin user {adminEmail} is already in the Admin role.");
    }
}



app.Run();


