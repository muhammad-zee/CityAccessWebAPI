using System;

#nullable disable

namespace Web.Data.Models
{
    public partial class CodeStrokeGroupMember
    {
        public int CodeStrokeGroupMemberId { get; set; }
        public int StrokeCodeIdFk { get; set; }
        public int UserIdFk { get; set; }
        public bool IsAcknowledge { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual CodeStroke StrokeCodeIdFkNavigation { get; set; }
    }
}
