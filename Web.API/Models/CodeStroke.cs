using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class CodeStroke
    {
        public CodeStroke()
        {
            CodeStrokeGroupMembers = new HashSet<CodeStrokeGroupMember>();
        }

        public int CodeStrokeId { get; set; }
        public string PatientName { get; set; }
        public int OrganizationIdFk { get; set; }
        public DateTime? Dob { get; set; }
        public int? Gender { get; set; }
        public string ChiefComplant { get; set; }
        public DateTime? LastKnownWell { get; set; }
        public string Hpi { get; set; }
        public string BloodThinners { get; set; }
        public string FamilyContactNumber { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool? IsEms { get; set; }
        public bool? IsCompleted { get; set; }
        public bool? IsActive { get; set; }
        public string FamilyContactName { get; set; }
        public string Audio { get; set; }
        public string Video { get; set; }
        public string Attachments { get; set; }
        public DateTime? EndTime { get; set; }
        public string EstimatedTime { get; set; }
        public TimeSpan? ActualTime { get; set; }
        public string Distance { get; set; }
        public string StartingPoint { get; set; }
        public string ChannelSid { get; set; }
        public long CodeStrokeNumber { get; set; }

        public virtual ICollection<CodeStrokeGroupMember> CodeStrokeGroupMembers { get; set; }
    }
}
