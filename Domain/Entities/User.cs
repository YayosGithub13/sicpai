using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("USERS")]
    public class User
    {
        public User()
        {

        }

        public User(string userName, string email, string passwordHash)
        {
            UserName = userName;
            Email = email;
            PasswordHash = passwordHash;
            LastUpdate = DateTime.UtcNow;
            ActiveFlag = true;
        }

        public int Id { get; set; }
        public string UserName { get; set; } = default!;
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? SecondLastName { get; set; }
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public DateTime LastUpdate { get; set; }
        public bool ActiveFlag { get; set; }

        public string FullName => $"{Name} {LastName} {SecondLastName}".Trim();

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}