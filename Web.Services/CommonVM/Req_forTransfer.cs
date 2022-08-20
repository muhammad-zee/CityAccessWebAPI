using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Web.Services.CommonVM
{
    public class Req_forTransfer
    {
        public int ID { get; set; }
        public int AgreementID { get; set; }
        public int BookerId { get; set; }

        [DisplayName("Price")]
        public decimal Price { get; set; }

        [DisplayName("Booking date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:dd-MM-yyyy}")]
        public Nullable<System.DateTime> BookingDate { get; set; }


        [DisplayName("Transfer date")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Mandatory Field.")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:dd-MM-yyyy}")]
        public Nullable<System.DateTime> EventDate { get; set; }
        //public System.DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Mandatory Field.")]
        [DisplayName("Pick up time")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public Nullable<System.TimeSpan> EventTime { get; set; }

        [DisplayName("Notes")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [DisplayName("Operator notes")]
        [DataType(DataType.MultilineText)]
        public string OperatorNotes { get; set; }

        [DisplayName("Client Notes")]
        [DataType(DataType.MultilineText)]
        public string ClientNotes { get; set; }

        [Required(ErrorMessage = "Mandatory Field.")]
        [DisplayName("Client name")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "Mandatory Field.")]
        [DisplayName("Client e-mail")]
        public string ContactEmail { get; set; }

        [Required(ErrorMessage = "Mandatory Field.")]
        [DisplayName("Client phone")]
        [StringLength(30)]
        //[RegularExpression("^[0-9]*$", ErrorMessage = "Phone must be numeric")]
        public string ContactPhone { get; set; }

        [DisplayName("Number of persons")]
        [Required(ErrorMessage = "Mandatory Field.")]
        public Nullable<int> NrPersons { get; set; }

        [DisplayName("Pick up location")]
        [Required(ErrorMessage = "Mandatory Field.")]
        public string PickupLocation { get; set; }

        [DisplayName("Drop off location")]
        [Required(ErrorMessage = "Mandatory Field.")]
        public string DropoffLocation { get; set; }

        [DisplayName("Flight number")]
        public string FlightNr { get; set; }

        [DisplayName("Status")]
        public string StateID { get; set; }

        [DisplayName("Reference")]
        public string Reference { get; set; }

        public string Aglabel { get; set; }
        public string Servlabel { get; set; }

        public string AgOp { get; set; } //string to have agent or operator tradename
        public Nullable<int> operatorID { get; set; }

        public byte[] logo { get; set; }

        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public Nullable<System.TimeSpan> Duration { get; set; }

        public Nullable<int> eventID { get; set; }


        public Nullable<int> TotalNrPersons { get; set; }
        public Nullable<decimal> TotalPrice { get; set; }

        [DataType(DataType.MultilineText)]
        public string EventNotes { get; set; }

        public string EventStatus { get; set; }

        public Nullable<int> Leg { get; set; }

        public Nullable<bool> HasReturn { get; set; }

        public Nullable<int> PartnerID { get; set; }


    }
}