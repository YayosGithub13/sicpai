using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<User?> ValidateUserAsync(string usernameOrEmail, string password);
        Task<User?> GetUserByIdAsync(int id);
        User? GetCurrentUser();
        Task<User> CreateUserAsync(User user, string password);
    }
}