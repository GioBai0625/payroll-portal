using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PayrollPortal.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure services
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()              // support roles
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 2. Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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

// 3. **SEED ROLES & DEFAULT HR USER** _before_ starting the server_
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // a) Ensure roles exist
    string[] roleNames = { "HR", "Employee" };
    foreach (var role in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // b) Ensure a default HR user exists
    string hrEmail = "acctg.supv3@rdfishing-png.com";
    string hrPassword = "gTb123!";

    var hrUser = await userManager.FindByEmailAsync(hrEmail);
    if (hrUser == null)
    {
        hrUser = new IdentityUser
        {
            UserName = hrEmail,
            Email = hrEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(hrUser, hrPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(hrUser, "HR");
        }
    }
}

// 4. Now start the app
app.Run();
