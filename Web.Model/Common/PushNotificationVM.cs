using System.Collections.Generic;

namespace Web.Model.Common
{
    public class PushNotificationVM
    {
        public int Id { get; set; }
        public int OrgId { get; set; }
        public List<string> UserChannelSid { get; set; }
        public string RouteLink { get; set; }
        public string RouteLinkEMS { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Msg { get; set; }

    }
}
