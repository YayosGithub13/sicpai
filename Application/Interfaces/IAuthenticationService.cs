using Domain.Entities;

namespace Application.Interfaces;

public interface IAuthenticationService
{
    Task<User?> ValidateCredentialsAsync(string usernameOrEmail, string password, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}