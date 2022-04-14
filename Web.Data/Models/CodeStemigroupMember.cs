using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class CodeStemigroupMember
    {
        public int CodeStemigroupMemberId { get; set; }
        public int StemicodeIdFk { get; set; }
        public int UserIdFk { get; set; }
        public bool IsAcknowledge { get; set; }
        public string ChannelSid { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual CodeStemi StemicodeIdFkNavigation { get; set; }
    }
}
