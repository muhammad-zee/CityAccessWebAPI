using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class Agreement
    {
        public Agreement()
        {
            AgreementLogs = new HashSet<AgreementLog>();
            Requests = new HashSet<Request>();
        }

        public int Id { get; set; }
        public int? PartnerId { get; set; }
        public int ServiceId { get; set; }
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

        public virtual CommissionType CommissionTypeNavigation { get; set; }
        public virtual Partner Partner { get; set; }
        public virtual DynamicFieldAlternative PaymentAgentTypeNavigation { get; set; }
        public virtual DynamicFieldAlternative PriceTypeNavigation { get; set; }
        public virtual Service Service { get; set; }
        public virtual DynamicFieldAlternative TypeCommissionNavigation { get; set; }
        public virtual ICollection<AgreementLog> AgreementLogs { get; set; }
        public virtual ICollection<Request> Requests { get; set; }
    }
}
