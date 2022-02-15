using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class PushNotificationVM
    {
        public int Id { get; set; }
        public int OrgId { get; set; }
        public List<string> UserChannelSid { get; set; }
        public string RouteLink { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Msg { get; set; }

    }
}
