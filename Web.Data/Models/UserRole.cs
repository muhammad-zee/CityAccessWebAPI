using System;
using System.Collections.Generic;
using Web.Model.Common;

#nullable disable

namespace Web.Data.Models
{
    public partial class UserRole
    {
        public string UserRoleId { get; set; }
        public string UserId { get; set; }
        public string RoleId { get; set; }

    }
}
