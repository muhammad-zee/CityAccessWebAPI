using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class AgreementVM
    {
        public int Id { get; set; }
        public int? PartnerId { get; set; }
        public string PartnerTradeName { get; set; }
        public string PartnerLogo { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal? ServicePrice { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public bool? IsActive { get; set; }
        public string MessageTemplate { get; set; }
        public string AgentInstructions { get; set; }
        public string CancellationPolicy { get; set; }
        public bool? NeedsApproval { get; set; }
        public decimal? Price { get; set; }
        public int? CommissionType { get; set; }
        public decimal? CommissionValue { get; set; }
        public bool? Override1 { get; set; }
        public decimal? PaymentAgent { get; set; }
        public string EmailToCustomer { get; set; }
        public int? PaymentAgentType { get; set; }
        public int? PriceType { get; set; }
        public int? TypeCommission { get; set; }
        public bool? IsConfirmed { get; set; }
        public byte[] ServiceImage { get; set; }
    }
}
