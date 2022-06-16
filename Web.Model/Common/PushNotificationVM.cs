using System.Collections.Generic;

namespace Web.Model.Common
{
    public class PushNotificationVM
    {
        public int Id { get; set; }
        public int OrgId { get; set; }
        public List<string> UserChannelSid { get; set; }
        public string RouteLink1 { get; set; }
        public string RouteLink2 { get; set; }
        public string RouteLink3 { get; set; }
        public string RouteLink4 { get; set; }
        public string RouteLink5 { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Msg { get; set; }
        public string ChannelSid { get; set; }
        public bool ChannelIsActive { get; set; }
        public string Type { get; set; }
        public bool ForEmsLocationUpdate { get; set; } = false;
        public string FieldName { get; set; }
        public object FieldValue { get; set; }
        public string FieldDataType { get; set; }
        public object RefreshModules { get; set; }

    }
}
