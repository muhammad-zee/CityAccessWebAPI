namespace Web.Model.Common
{
    public class UserRoleVM
    {
        //public int UserRoleId { get; set; }
        //public int UserId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int? OrganizationIdFk { get; set; }
        public string OrganizationName { get; set; }
        public bool IsSuperAdmin { get; set; }

    }
}
