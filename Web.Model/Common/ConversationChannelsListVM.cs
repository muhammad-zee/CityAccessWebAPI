namespace Web.Model.Common
{
    public class ConversationChannelsListVM
    {
        public string ChannelFriendlyName { get; set; }
        public string ChannelUniqueName { get; set; }
        public string ParticipantFriendlyName { get; set; }
        public string ParticipantUniqueName { get; set; }
        public int ParticipantUserId { get; set; }
        public string ConversationImage { get; set; }
        public string ChannelSid { get; set; }
        public bool? IsGroup { get; set; }
        public int CreatedBy { get; set; }
    }
}
