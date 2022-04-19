using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class Role
    {
        public Role()
        {
            ComponentAccesses = new HashSet<ComponentAccess>();
            UserAccesses = new HashSet<UserAccess>();
            UserRoles = new HashSet<UserRole>();
            UsersSchedules = new HashSet<UsersSchedule>();
        }

        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int? OrganizationIdFk { get; set; }
        public string RoleDescription { get; set; }
        public string RoleDiscrimination { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsScheduleRequired { get; set; }
        public bool IsSuperAdmin { get; set; }

        public virtual ICollection<ComponentAccess> ComponentAccesses { get; set; }
        public virtual ICollection<UserAccess> UserAccesses { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<UsersSchedule> UsersSchedules { get; set; }
    }
}
