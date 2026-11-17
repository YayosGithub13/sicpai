using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly AppDbContext _dbContext;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthenticationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> ValidateCredentialsAsync(string usernameOrEmail, string password, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.RoleCatalog)
            .ThenInclude(rc => rc.RolePermissions)
            .ThenInclude(rp => rp.PermissionCatalog)
            .FirstOrDefaultAsync(u => u.Email == usernameOrEmail || u.UserName == usernameOrEmail, cancellationToken);
        if (user is null || !user.ActiveFlag || user.LockoutEnd > DateTime.UtcNow) return null;

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Success)
        {
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            user.LastUpdate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return user;
        }

        user.FailedLoginAttempts++;
        if (user.FailedLoginAttempts >= 5)
        {
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            user.FailedLoginAttempts = 0;
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
        return null;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword) == PasswordVerificationResult.Failed)
            return false;
        user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        user.LastUpdate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}