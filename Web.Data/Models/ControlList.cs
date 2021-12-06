using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class ControlList
    {
        public ControlList()
        {
            ControlListDetails = new HashSet<ControlListDetail>();
        }

        public int ControlListId { get; set; }
        public string ControlListType { get; set; }
        public string ControlListTitle { get; set; }
        public bool ControlListIsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<ControlListDetail> ControlListDetails { get; set; }
    }
}
