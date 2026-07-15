using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Seed
{
    public static class DataSeeder
    {
        public static void SeedInitialData(AppDbContext context)
        {
            // Ensure database is created (for simple scenarios). For production use migrations.
            context.Database.EnsureCreated();

            if (!context.RoleCatalogs.Any())
            {
                var roles = new List<RoleCatalog>
                {
                    new RoleCatalog { Name = "Administrator", Description = "Administrador del sistema", LastUpdate = DateTime.UtcNow, ActiveFlag = true },
                    new RoleCatalog { Name = "User", Description = "Usuario regular", LastUpdate = DateTime.UtcNow, ActiveFlag = true }
                };

                context.RoleCatalogs.AddRange(roles);
                context.SaveChanges();
            }

            if (!context.Users.Any())
            {
                var hasher = new PasswordHasher<User>();
                var admin = new User("admin", "admin@sicpai.local", string.Empty)
                {
                    Name = "Admin",
                    LastName = "System",
                    LastUpdate = DateTime.UtcNow,
                    ActiveFlag = true
                };

                admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
                context.Users.Add(admin);
                context.SaveChanges();

                var adminRole = context.RoleCatalogs.FirstOrDefault(r => r.Name == "Administrator");
                if (adminRole != null)
                {
                    context.UserRoles.Add(new UserRole { UserId = admin.Id, RoleCatalogId = adminRole.Id, LastUpdate = DateTime.UtcNow, ActiveFlag = true });
                    context.SaveChanges();
                }
            }
        }
    }
}