using System;

#nullable disable

namespace Web.Data.Models
{
    public partial class CommunicationLog
    {
        public int CommunicationLogId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SentFrom { get; set; }
        public string SentTo { get; set; }
        public int? ServiceLineIdFk { get; set; }
        public int? LogType { get; set; }
        public string Direction { get; set; }
        public string MediaUrl { get; set; }
        public string UniqueSid { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDelete { get; set; }
    }
}
