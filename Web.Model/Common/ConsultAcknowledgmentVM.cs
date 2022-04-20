namespace Web.Model.Common
{
    public class ConsultAcknowledgmentVM
    {
        public long consultid { get; set; }
        public long consultNumber { get; set; }
        public string userName { get; set; }
        public string patientName { get; set; }
        public string serviceName { get; set; }
        public bool isAcknowledge { get; set; }
        public string roleName { get; set; }
        public int userIdFk { get; set; }
        public string userUniqueId { get; set; }
        public string channelSid { get; set; }
    }
}
