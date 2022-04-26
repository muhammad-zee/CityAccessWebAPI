using System.Collections.Generic;
using Web.Data.Models;

namespace Web.Services.Interfaces
{
    public interface IActiveCodeHelperService
    {

        void MemberAddedToConversationChannel(List<ConversationParticipant> channelParticipantsList, string ChannelSid);
    }
}
