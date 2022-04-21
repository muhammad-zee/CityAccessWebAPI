using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Data.Models;

namespace Web.Services.Interfaces
{
    public interface IActiveCodeHelperService
    {

        void MemberAddedToConversationChannel(List<ConversationParticipant> channelParticipantsList, string ChannelSid);
}
}
