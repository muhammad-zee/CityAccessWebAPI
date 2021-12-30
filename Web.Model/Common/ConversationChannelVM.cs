using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class ConversationChannelVM
    {
        public string FriendlyName { get; set; }
        public string UniqueName { get; set; }
        public string ChannelSid { get; set; }
        public bool? IsGroup { get; set; }
        public int CreatedBy { get; set; }
    }
}
