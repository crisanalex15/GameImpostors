using Microsoft.AspNetCore.Identity;
using Backend.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend
{
    public class MakeUserAdmin
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Set DataDirectory
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(builder.Environment.ContentRootPath, "App_Data"));
            
            // Add DbContext service
            builder.Services.AddDbContext<AuthDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseSqlite(connectionString);
            });
            
            // Configure Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();
            
            var app = builder.Build();
            
            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                
                // Create Admin role if it doesn't exist
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                    Console.WriteLine("Admin role created.");
                }
                
                // Find user by email
                var user = await userManager.FindByEmailAsync("example@test.com");
                if (user != null)
                {
                    // Add user to Admin role
                    if (!await userManager.IsInRoleAsync(user, "Admin"))
                    {
                        await userManager.AddToRoleAsync(user, "Admin");
                        Console.WriteLine($"User {user.Email} added to Admin role.");
                    }
                    else
                    {
                        Console.WriteLine($"User {user.Email} is already an Admin.");
                    }
                }
                else
                {
                    Console.WriteLine("User example@test.com not found.");
                }
            }
        }
    }
}


