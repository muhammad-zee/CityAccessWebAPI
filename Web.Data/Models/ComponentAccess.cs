using System;

#nullable disable

namespace Web.Data.Models
{
    public partial class ComponentAccess
    {
        public int ComponentAccessId { get; set; }
        public int RoleIdFk { get; set; }
        public int ComponentIdFk { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }

        public virtual Component ComponentIdFkNavigation { get; set; }
        public virtual Role RoleIdFkNavigation { get; set; }
    }
}
