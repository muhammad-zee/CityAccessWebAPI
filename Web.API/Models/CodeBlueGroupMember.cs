using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class CodeBlueGroupMember
    {
        public int CodeBlueGroupMemberId { get; set; }
        public int BlueCodeIdFk { get; set; }
        public int UserIdFk { get; set; }
        public bool IsAcknowledge { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual CodeBlue BlueCodeIdFkNavigation { get; set; }
    }
}
