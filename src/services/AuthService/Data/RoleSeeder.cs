using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public static class RoleSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        var roles = new[] { ("User", "Standard user"), ("Admin", "Administrator") };

        foreach (var (name, description) in roles)
        {
            if (!await db.Roles.AnyAsync(r => r.Name == name))
                db.Roles.Add(new Role { Name = name, Description = description });
        }

        await db.SaveChangesAsync();
    }
}
