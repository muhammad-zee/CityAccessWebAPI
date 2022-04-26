namespace Web.Model.Common
{
    public class ReservationsVM
    {
        public int ReservationId { get; set; }
        public int QueueIdFk { get; set; } = 0;
        public int ReservationStatus { get; set; } = 0;
        public string ReservationAssignedTo { get; set; } = "";
        public string CallSid { get; set; } = "";
    }
}
