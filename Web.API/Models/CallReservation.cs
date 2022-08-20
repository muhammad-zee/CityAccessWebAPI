using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class CallReservation
    {
        public int ReservationId { get; set; }
        public int QueueIdFk { get; set; }
        public int ReservationStatus { get; set; }
        public string ReservationAssignedTo { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CallSid { get; set; }

        public virtual CallQueue QueueIdFkNavigation { get; set; }
    }
}
