using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class CallQueue
    {
        public CallQueue()
        {
            CallReservations = new HashSet<CallReservation>();
        }

        public int QueueId { get; set; }
        public string FromPhoneNumber { get; set; }
        public string ToPhoneNumber { get; set; }
        public int ServiceLineIdFk { get; set; }
        public int RoleIdFk { get; set; }
        public string CallSid { get; set; }
        public string ParentCallSid { get; set; }
        public string ConferenceSid { get; set; }
        public int QueueStatus { get; set; }
        public int? QueueAcceptedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<CallReservation> CallReservations { get; set; }
    }
}
