using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class Request
    {
        public Request()
        {
            RequestLogs = new HashSet<RequestLog>();
        }

        public int Id { get; set; }
        public int AgreementId { get; set; }
        public int BookerId { get; set; }
        public decimal Price { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan EventTime { get; set; }
        public string Notes { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public int? NrPersons { get; set; }
        public string PickupLocation { get; set; }
        public string DropoffLocation { get; set; }
        public string ReturnPickup { get; set; }
        public string ReturnDropoff { get; set; }
        public string FlightNr { get; set; }
        public DateTime? ReturnDate { get; set; }
        public TimeSpan? ReturnTime { get; set; }
        public string ReturnFlight { get; set; }
        public string StateId { get; set; }
        public string Reference { get; set; }
        public DateTime? BookDate { get; set; }
        public TimeSpan? BookTime { get; set; }
        public DateTime? ExtraDate1 { get; set; }
        public DateTime? ExtraDate2 { get; set; }
        public DateTime? ExtraDate3 { get; set; }
        public TimeSpan? ExtraTime1 { get; set; }
        public TimeSpan? ExtraTime2 { get; set; }
        public TimeSpan? ExtraTime3 { get; set; }
        public string ExtraText1 { get; set; }
        public string ExtraText2 { get; set; }
        public string ExtraText3 { get; set; }
        public string ExtraMultiText1 { get; set; }
        public string ExtraMultiText2 { get; set; }
        public string ExtraMultiText3 { get; set; }
        public string OperatorNotes { get; set; }
        public int? EventId { get; set; }
        public int? ResponsibleId { get; set; }
        public string ClientNotes { get; set; }

        public virtual Agreement Agreement { get; set; }
        public virtual User Booker { get; set; }
        public virtual Event Event { get; set; }
        public virtual User Responsible { get; set; }
        public virtual State State { get; set; }
        public virtual ICollection<RequestLog> RequestLogs { get; set; }
    }
}
