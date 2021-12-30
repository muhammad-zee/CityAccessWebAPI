using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class ConversationChannelParticipantsVM
    {
        public string ChannelSid { get; set; }
        public int UserId { get; set; }
        public int CreatedBy { get; set; }
        public List<string> Participants { get; set; }
    }
}

