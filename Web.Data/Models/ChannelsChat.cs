using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class ChannelsChat
    {
        public ChannelsChat()
        {
            ChannelsMembersChats = new HashSet<ChannelsMembersChat>();
        }

        public int ChannelsChatId { get; set; }
        public string FriendlyName { get; set; }
        public string UniqueName { get; set; }
        public int? ChannelSid { get; set; }
        public bool? IsGroup { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<ChannelsMembersChat> ChannelsMembersChats { get; set; }
    }
}
