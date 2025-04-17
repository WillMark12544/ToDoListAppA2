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
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews(); //Views

builder.Services.ConfigureApplicationCookie(options => 
{
    options.LoginPath = "/Identity/Account/Login";
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

app.Use(async (context, next) =>
{
    if (context.User.Identity.IsAuthenticated) //Checks identity
    {
        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
    }
    await next();
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); 

app.Run();
