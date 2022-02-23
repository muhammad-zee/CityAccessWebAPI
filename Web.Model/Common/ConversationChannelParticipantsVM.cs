using System.Collections.Generic;

namespace Web.Model.Common
{
    public class ConversationChannelParticipantsVM
    {
        public string ChannelSid { get; set; }
        public int UserId { get; set; }
        public int CreatedBy { get; set; }
        public bool IsAdmin { get; set; }
        public List<ParticipantVM> Participants { get; set; }
    }

    public class ParticipantVM
    {
        public string FriendlyName{get;set; }
        public string UniqueName{get;set;}
        public string ParticipantSid{get;set;}
        public string ChannelSid{ get; set; }
        public bool IsAdmin { get; set; }
    }
}

