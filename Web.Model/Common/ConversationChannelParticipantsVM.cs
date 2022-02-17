using System.Collections.Generic;

namespace Web.Model.Common
{
    public class ConversationChannelParticipantsVM
    {
        public string ChannelSid { get; set; }
        public int UserId { get; set; }
        public int CreatedBy { get; set; }
        public bool IsAdmin { get; set; }
        public List<string> Participants { get; set; }
    }
}

