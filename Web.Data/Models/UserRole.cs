#nullable disable

namespace Web.Data.Models
{
    public partial class UserRole
    {
        public int UserRoleId { get; set; }
        public int UserIdFK { get; set; }
        public int RoleIdFK { get; set; }
    }
}
