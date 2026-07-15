using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("USERROLES")]
    public class UserRole
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleCatalogId { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool ActiveFlag { get; set; }

        public User User { get; set; } = default!;
        public RoleCatalog RoleCatalog { get; set; } = default!;
    }
}