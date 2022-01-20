using System;

#nullable disable

namespace Web.Data.Models
{
    public partial class ControlListDetail
    {
        public int ControlListDetailId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsLocked { get; set; }
        public int SortOrder { get; set; }
        public string UniqueId { get; set; }
        public int ControlListIdFk { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ControlList ControlListIdFkNavigation { get; set; }
    }
}
