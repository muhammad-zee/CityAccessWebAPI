using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class ServicesVM
    {
        public int ServiceId { get; set; }
        public int ServiceTypeId { get; set; }
        [JsonIgnore]
        public string ServiceTypeName { get; set; }
        public string  ServiceName { get; set; }
        public string Descritpion { get; set; }
        public int CityId { get; set; }
        [JsonIgnore]
        public string CityName { get; set; }
        public bool IsPublic { get; set; }
        public int PartnerId { get; set; }
        public string PartnerTradeName { get; set; }
        public int? MaxNumberOfPersons { get; set; }
        public int? MinNumberOfPersons { get; set; }
        public bool? OverridePrice { get; set; }
        public decimal? PaymentAgent { get; set; }
        public string AgentInstructions { get; set; }
        public string ConfirmationText { get; set; }
        public string CancellationPolicy { get; set; }
        public decimal? CommissionValue { get; set; }
        public decimal? Price { get; set; }
        public int PriceTypeId { get; set; }
        [JsonIgnore]
        public string PriceTypeLabel { get; set; }
        public int? CommissionTypeId { get; set; }
        [JsonIgnore]
        public string CommissionTypeName { get; set; }
        public int PaymentAgentTypeId { get; set; }
        [JsonIgnore]
        public string PaymentAgentTypeLabel { get; set; }
        public int? AvailabilityId { get; set; }
        [JsonIgnore]
        public string AvailabilityLabel { get; set; }
        public string ServiceImage { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:hh\:mm}")]
        public TimeSpan Duration { get; set; }










    }
}
