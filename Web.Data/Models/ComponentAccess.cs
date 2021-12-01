using System;

#nullable disable

namespace Web.Data.Models
{
    public partial class ComponentAccess
    {
        public int ComponentAccessId { get; set; }
        public string RoleIdFk { get; set; }
        public int? ComIdFk { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool Active { get; set; }
    }
}
