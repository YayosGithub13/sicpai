using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<User> CreateUserAsync(User user, string password)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            user.LastUpdate = DateTime.UtcNow;
            user.ActiveFlag = true;

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _dbContext.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.RoleCatalog).FirstOrDefaultAsync(u => u.Id == id);
        }

        public User? GetCurrentUser()
        {
            // This method is intended to be used from controllers that have access to HttpContext.
            // For infrastructure-level service we cannot access HttpContext directly.
            // The WebUI should implement a wrapper to resolve the current user from ClaimsPrincipal.
            return null;
        }

        public async Task<User?> ValidateUserAsync(string usernameOrEmail, string password)
        {
            var user = await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.RoleCatalog)
                .ThenInclude(rc => rc.RolePermissions)
                .ThenInclude(rp => rp.PermissionCatalog)
                .FirstOrDefaultAsync(u => u.Email == usernameOrEmail || u.UserName == usernameOrEmail);
            if (user == null) return null;
            if (!user.ActiveFlag || user.LockoutEnd > DateTime.UtcNow) return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Success)
            {
                user.FailedLoginAttempts = 0;
                user.LockoutEnd = null;
                user.LastUpdate = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                return user;
            }

            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                user.FailedLoginAttempts = 0;
            }
            await _dbContext.SaveChangesAsync();
            return null;
        }
    }
}