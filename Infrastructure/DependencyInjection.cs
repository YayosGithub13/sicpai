using System;
using Infrastructure.Data;
using Infrastructure.Services;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces;
using Application.Services;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                   ?? configuration["ConnectionStrings:DefaultConnection"]
                                   ?? configuration["DefaultConnection"];

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAssetRepository, AssetRepository>();
            services.AddScoped<IAssetService, AssetService>();
            services.AddScoped<IActivityLogWriter, ActivityLogWriter>();
            services.AddScoped<ITransferService, TransferService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<ICatalogService, CatalogService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IReportAssetService, ReportAssetService>();
            services.AddScoped<IInventoryOperations, InventoryOperations>();
            services.AddScoped<IAuditOperations, AuditOperations>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ICatalogOperations, CatalogOperations>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}
