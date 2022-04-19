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
        public string FromPhoneNumber { get; set; } = "";
        public string ToPhoneNumber { get; set; } = "";
        public int ServiceLineIdFk { get; set; } = 0;
        public int RoleIdFk { get; set; } = 0;
        public string Callsid { get; set; } = "";
        public string ParentCallsid { get; set; } = "";
        public string ConfrenceSid { get; set; } = "";
        public int QueueStatus { get; set; } = 0;
        public int? QueueAcceptedBy { get; set; } = 0;

    }
}
