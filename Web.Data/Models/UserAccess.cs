using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class UserAccess
    {
        public int UserAccessId { get; set; }
        public int UserIdFk { get; set; }
        public int RoleIdFk { get; set; }
        public int ComponentIdFk { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Role RoleIdFkNavigation { get; set; }
        public virtual User UserIdFkNavigation { get; set; }
    }
}
