using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class ConversationChannel
    {
        public ConversationChannel()
        {
            ConversationParticipants = new HashSet<ConversationParticipant>();
        }

        public int ConversationChannelId { get; set; }
        public string FriendlyName { get; set; }
        public string UniqueName { get; set; }
        public string ChannelSid { get; set; }
        public bool? IsGroup { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<ConversationParticipant> ConversationParticipants { get; set; }
    }
}
