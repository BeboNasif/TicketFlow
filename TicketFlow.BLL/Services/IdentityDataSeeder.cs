namespace TicketFlow.BLL.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TicketFlow.DAL.Models;

public static class IdentityDataSeeder
{
    public static async Task SeedIdentityDataAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentityDataSeeder");
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        try
        {
            // 1. Seed Roles
            string[] roles = ["Admin", "Organizer", "Customer"];

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                    if (!roleResult.Succeeded)
                    {
                        logger.LogError("Failed to create role '{RoleName}': {Errors}",
                            roleName, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }
            }

            if (!await userManager.Users.AnyAsync())
            {
                var adminUser = new AppUser
                {
                    UserName = "admin@ticketflow.com",
                    Email = "admin@ticketflow.com",
                    FullName = "System Admin",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var userResult = await userManager.CreateAsync(adminUser, "Admin@123456");

                if (userResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Identity Data Seeded: Default Admin user created successfully.");
                }
                else
                {
                    logger.LogError("Failed to create Admin user: {Errors}",
                        string.Join(", ", userResult.Errors.Select(e => e.Description)));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding Identity data.");
        }
    }
}