using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class AgreementVM
    {
        public int AgreementId { get; set; }
        public int? PartnerId { get; set; }
        [JsonIgnore]
        public string PartnerTradeName { get; set; }
        [JsonIgnore]
        public string PartnerLogo { get; set; }
        public int ServiceId { get; set; }
        [JsonIgnore]
        public string ServiceName { get; set; }
        public decimal? ServicePrice { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string MessageTemplate { get; set; }
        public string AgentInstructions { get; set; }
        public string CancellationPolicy { get; set; }
        public bool? NeedsApproval { get; set; }
        public decimal? AgreementPrice { get; set; }
        public int? CommissionTypeId { get; set; }
        [JsonIgnore]
        public string CommissionTypeName { get; set; }
        public decimal? CommissionValue { get; set; }
        public bool? Override1 { get; set; }
        public decimal? PaymentAgent { get; set; }
        public string EmailToCustomer { get; set; }
        public int? PaymentAgentTypeId { get; set; }
        [JsonIgnore]
        public string PaymentAgentTypeLabel { get; set; }
        public int? PriceTypeId { get; set; }
        public string PriceTypeLabel { get; set; }
        public bool? IsConfirmed { get; set; }
        public string ServiceImage { get; set; }
    }
}
