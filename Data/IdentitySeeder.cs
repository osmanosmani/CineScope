using Microsoft.AspNetCore.Identity;

namespace CineScope.Data;

public static class IdentitySeeder
{
    public const string AdminRole = "Admin";
    public const string MemberRole = "Member";
    public const string AdminEmail = "admin@cinescope.com";
    public const string AdminPassword = "Admin123!";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await EnsureRoleAsync(roleManager, AdminRole);
        await EnsureRoleAsync(roleManager, MemberRole);
        await EnsureAdminUserAsync(userManager);
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Could not create role '{roleName}': {GetErrors(result)}");
            }
        }
    }

    private static async Task EnsureAdminUserAsync(UserManager<IdentityUser> userManager)
    {
        var adminUser = await userManager.FindByEmailAsync(AdminEmail);

        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, AdminPassword);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"Could not create default admin user: {GetErrors(createResult)}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, AdminRole);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException($"Could not assign admin role: {GetErrors(roleResult)}");
            }
        }
    }

    private static string GetErrors(IdentityResult result)
    {
        return string.Join("; ", result.Errors.Select(error => error.Description));
    }
}
