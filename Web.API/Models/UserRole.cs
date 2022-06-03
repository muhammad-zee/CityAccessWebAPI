using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class UserRole
    {
        public int UserRoleId { get; set; }
        public int UserIdFk { get; set; }
        public int RoleIdFk { get; set; }

        public virtual Role RoleIdFkNavigation { get; set; }
        public virtual User UserIdFkNavigation { get; set; }
    }
}
