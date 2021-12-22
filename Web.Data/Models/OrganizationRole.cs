#nullable disable

namespace Web.Data.Models
{
    public partial class OrganizationRole
    {
        public int OrganizationRoleId { get; set; }
        public int OrganizationIdFk { get; set; }
        public int RoleIdFk { get; set; }

        public virtual Organization OrganizationIdFkNavigation { get; set; }
        public virtual Role RoleIdFkNavigation { get; set; }
    }
}
