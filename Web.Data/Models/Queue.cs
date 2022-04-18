using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class Queue
    {
        public int QueueId { get; set; }
        public string FromPhoneNumber { get; set; }
        public string ToPhoneNumber { get; set; }
        public string Callsid { get; set; }
        public string ParentCallsid { get; set; }
        public string ConfrenceSid { get; set; }
        public string QueueStatus { get; set; }
        public int? QueueAcceptedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int ServiceLineIdFk { get; set; }
        public int RoleIdFk { get; set; }
    }
}
