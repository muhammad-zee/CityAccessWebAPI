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
        public string AgreementName { get; set; }
        public decimal Price { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Mandatory Field.")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:yyyy-MM-dd}")]
        public System.DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Mandatory Field.")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public System.TimeSpan EventTime { get; set; }

        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        public string ClientNotes { get; set; }

        [Required(ErrorMessage = "Mandatory Field.")]
        public string ContactName { get; set; }


        public string ContactEmail { get; set; }


        [StringLength(30)]
        //[RegularExpression("^[0-9]*$", ErrorMessage = "Phone must be numeric")]
        public string ContactPhone { get; set; }

        [Required(ErrorMessage = "Mandatory Field.")]
        public Nullable<int> NrPersons { get; set; }

        public string PickupLocation { get; set; }

        public string DropoffLocation { get; set; }

        public string FlightNr { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:yyyy-MM-dd}")]
        public Nullable<System.DateTime> returnDate { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        [DataType(DataType.Time)]
        public Nullable<System.TimeSpan> returnTime { get; set; }

        public string ReturnFlight { get; set; }

        public string StateID { get; set; }

        public string Reference { get; set; }

        public string ReturnPickup { get; set; }

        public string ReturnDropoff { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:yyyy-MM-dd}")]
        public Nullable<System.DateTime> BookDate { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
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
        public bool? FromPartnerSite { get; set; } = null;
        public string PartnerName { get; set; }
        public string ServiceImage { get; set; }
    }
}
