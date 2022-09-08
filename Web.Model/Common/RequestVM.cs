using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class RequestVM
    {
        public int ID { get; set; }
        public int AgreementID { get; set; }

        [DisplayName("Price")]
        public decimal Price { get; set; }

        [DisplayName("Date")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Mandatory Field.")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:yyyy-MM-dd}")]
        public System.DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Mandatory Field.")]
        [DisplayName("Time")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public System.TimeSpan EventTime { get; set; }

        [DisplayName("Notes")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [DisplayName("Client Notes")]
        public string ClientNotes { get; set; }

        [Required(ErrorMessage = "Mandatory Field.")]
        [DisplayName("Client name")]
        public string ContactName { get; set; }


        [DisplayName("Client e-mail")]
        public string ContactEmail { get; set; }


        [DisplayName("Client phone")]
        [StringLength(30)]
        //[RegularExpression("^[0-9]*$", ErrorMessage = "Phone must be numeric")]
        public string ContactPhone { get; set; }

        [DisplayName("Number of persons")]
        [Required(ErrorMessage = "Mandatory Field.")]
        public Nullable<int> NrPersons { get; set; }

        [DisplayName("Pick up location")]
        public string PickupLocation { get; set; }

        [DisplayName("Drop off location")]
        public string DropoffLocation { get; set; }

        [DisplayName("Flight number")]
        public string FlightNr { get; set; }

        [DisplayName("Return  date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:yyyy-MM-dd}")]
        public Nullable<System.DateTime> returnDate { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        [DisplayName("Return time")]
        [DataType(DataType.Time)]
        public Nullable<System.TimeSpan> returnTime { get; set; }

        [DisplayName("Return flight number")]
        public string ReturnFlight { get; set; }

        [DisplayName("Status")]
        public string StateID { get; set; }

        [DisplayName("Reference")]
        public string Reference { get; set; }

        [DisplayName("Return pick up location")]
        public string ReturnPickup { get; set; }

        [DisplayName("Return drop off location")]
        public string ReturnDropoff { get; set; }

        [DisplayName("Booking  date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:yyyy-MM-dd}")]
        public Nullable<System.DateTime> BookDate { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        [DisplayName("Booking time")]
        [DataType(DataType.Time)]
        public Nullable<System.TimeSpan> BookTime { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:yyyy-MM-dd}")]
        public Nullable<System.DateTime> ExtraDate1 { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:yyyy-MM-dd}")]
        public Nullable<System.DateTime> ExtraDate2 { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        [DataType(DataType.Time)]
        public Nullable<System.TimeSpan> ExtraTime1 { get; set; }

        [DataType(DataType.MultilineText)]
        public string ExtraMultiText1 { get; set; }
        public string ExtraText1 { get; set; }
    }
}
