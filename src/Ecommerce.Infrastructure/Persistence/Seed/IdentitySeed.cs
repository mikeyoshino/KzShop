using Ecommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Infrastructure.Persistence.Seed;

public static class IdentitySeed
{
    public const string DevelopmentAdminEmail = "admin@example.com";
    public const string DevelopmentAdminPassword = "Admin123!";
    public const string DevelopmentMikeyAdminEmail = "mikeyoshinos@gmail.com";
    public const string DevelopmentMikeyAdminPassword = "Mikey123";
    public const string AdminRole = "admin";

    public static async Task SeedDevelopmentAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            var roleResult = await roleManager.CreateAsync(new ApplicationRole { Name = AdminRole });
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(BuildErrorMessage(roleResult.Errors));
            }
        }

        await EnsureAdminAsync(userManager, DevelopmentAdminEmail, DevelopmentAdminPassword);
        await EnsureAdminAsync(userManager, DevelopmentMikeyAdminEmail, DevelopmentMikeyAdminPassword);
    }

    private static async Task EnsureAdminAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password)
    {
        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(admin, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(BuildErrorMessage(createResult.Errors));
            }
        }

        if (!await userManager.IsInRoleAsync(admin, AdminRole))
        {
            var addRoleResult = await userManager.AddToRoleAsync(admin, AdminRole);
            if (!addRoleResult.Succeeded)
            {
                throw new InvalidOperationException(BuildErrorMessage(addRoleResult.Errors));
            }
        }
    }

    private static string BuildErrorMessage(IEnumerable<IdentityError> errors)
    {
        return string.Join("; ", errors.Select(x => x.Description));
    }
}
