using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class ConversationMessageVM
    {

        public string author { get; set; }
        public string body { get; set; }
        public string attributes { get; set; }
        public string channelSid { get; set; }
    }
}
