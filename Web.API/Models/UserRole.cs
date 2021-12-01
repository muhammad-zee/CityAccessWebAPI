using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class UserRole
    {
        public int UserRoleId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }
}
