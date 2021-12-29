using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class ChannelsMembersChat
    {
        public int ChannelsMembersChatId { get; set; }
        public string FriendlyName { get; set; }
        public string UniqueName { get; set; }
        public int UserIdFk { get; set; }
        public int ChannelsChatIdFk { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ChannelsChat ChannelsChatIdFkNavigation { get; set; }
        public virtual User UserIdFkNavigation { get; set; }
    }
}
