using System;

#nullable disable

namespace Web.Data.Models
{
    public partial class ActiveCodesGroupMember
    {
        public int ActiveCodesGroupMemberId { get; set; }
        public long ActiveCodeIdFk { get; set; }
        public int UserIdFk { get; set; }
        public bool IsAcknowledge { get; set; }
        public string ActiveCodeName { get; set; }
        public string ChannelSid { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
