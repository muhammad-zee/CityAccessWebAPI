using System.Collections.Generic;

namespace Web.Model.Common
{
    public class ComponentAccessUserRoleVM
    {
        public int RoleId { get; set; }
        public int UserId { get; set; }
        public int LoggedInUserId { get; set; }
        public List<attrVM> Attributes { get; set; }
    }
    public class attrVM
    {
        public int id { get; set; }
        public bool selected { get; set; }
    }
}
