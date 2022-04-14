using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class QueuesVM
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

    }
}
