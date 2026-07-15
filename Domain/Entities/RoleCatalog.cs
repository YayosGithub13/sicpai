using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("USERCATALOGS")]
    public class RoleCatalog
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool ActiveFlag { get; set; }

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}