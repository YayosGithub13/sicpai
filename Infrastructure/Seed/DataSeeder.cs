using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seed
{
    public static class DataSeeder
    {
        public static void SeedInitialData(AppDbContext context)
        {
            // The first version used EnsureCreated. Baseline those databases before applying new migrations.
            context.Database.ExecuteSqlRaw(@"
                IF OBJECT_ID(N'dbo.USERS', N'U') IS NOT NULL
                BEGIN
                    IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
                    BEGIN
                        CREATE TABLE [__EFMigrationsHistory] (
                            [MigrationId] nvarchar(150) NOT NULL,
                            [ProductVersion] nvarchar(32) NOT NULL,
                            CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                        );
                    END;
                    IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260715172052_InitialCreate')
                        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                        VALUES (N'20260715172052_InitialCreate', N'8.0.0');
                END");
            context.Database.Migrate();

            if (!context.RoleCatalogs.Any())
            {
                var roles = new List<RoleCatalog>
                {
                    new RoleCatalog { Name = "Administrator", Description = "Administrador del sistema", LastUpdate = DateTime.UtcNow, ActiveFlag = true },
                    new RoleCatalog { Name = "Capturista", Description = "Captura y actualización de bienes", LastUpdate = DateTime.UtcNow, ActiveFlag = true },
                    new RoleCatalog { Name = "Supervisor", Description = "Supervisión operativa", LastUpdate = DateTime.UtcNow, ActiveFlag = true },
                    new RoleCatalog { Name = "Auditor", Description = "Auditoría institucional", LastUpdate = DateTime.UtcNow, ActiveFlag = true },
                    new RoleCatalog { Name = "Consultor", Description = "Consulta de información", LastUpdate = DateTime.UtcNow, ActiveFlag = true }
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

            var permissions = new[]
            {
                ("assets.read", "Consultar bienes", "Bienes"), ("assets.create", "Registrar bienes", "Bienes"), ("assets.update", "Actualizar bienes", "Bienes"), ("assets.delete", "Dar de baja bienes", "Bienes"),
                ("audits.manage", "Gestionar auditorías", "Auditorías"), ("transfers.approve", "Aprobar transferencias", "Transferencias"), ("users.manage", "Gestionar usuarios", "Administración"), ("reports.export", "Exportar reportes", "Reportes")
            };
            foreach (var permission in permissions)
            {
                if (!context.PermissionCatalogs.Any(p => p.Code == permission.Item1)) context.PermissionCatalogs.Add(new PermissionCatalog { Code = permission.Item1, Name = permission.Item2, Module = permission.Item3 });
            }
            context.SaveChanges();
            var administrator = context.RoleCatalogs.First(r => r.Name == "Administrator");
            var permissionIds = context.PermissionCatalogs.Select(p => p.Id).ToList();
            foreach (var permissionId in permissionIds)
                if (!context.RolePermissions.Any(rp => rp.RoleCatalogId == administrator.Id && rp.PermissionCatalogId == permissionId)) context.RolePermissions.Add(new RolePermission { RoleCatalogId = administrator.Id, PermissionCatalogId = permissionId });
            context.SaveChanges();
        }
    }
}