using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class DynamicFieldAlternative
    {
        public DynamicFieldAlternative()
        {
            AgreementPaymentAgentTypeNavigations = new HashSet<Agreement>();
            AgreementPriceTypeNavigations = new HashSet<Agreement>();
            AgreementTypeCommissionNavigations = new HashSet<Agreement>();
            DynamicFieldAlternativeMls = new HashSet<DynamicFieldAlternativeMl>();
            ServiceAvailability1Navigations = new HashSet<Service>();
            ServiceComissionTypeNavigations = new HashSet<Service>();
            ServicePaymentAgentTypeNavigations = new HashSet<Service>();
            ServicePriceTypeNavigations = new HashSet<Service>();
        }

        public int Id { get; set; }
        public string Label { get; set; }
        public int DynamicfieldId { get; set; }

        public virtual DynamicField Dynamicfield { get; set; }
        public virtual ICollection<Agreement> AgreementPaymentAgentTypeNavigations { get; set; }
        public virtual ICollection<Agreement> AgreementPriceTypeNavigations { get; set; }
        public virtual ICollection<Agreement> AgreementTypeCommissionNavigations { get; set; }
        public virtual ICollection<DynamicFieldAlternativeMl> DynamicFieldAlternativeMls { get; set; }
        public virtual ICollection<Service> ServiceAvailability1Navigations { get; set; }
        public virtual ICollection<Service> ServiceComissionTypeNavigations { get; set; }
        public virtual ICollection<Service> ServicePaymentAgentTypeNavigations { get; set; }
        public virtual ICollection<Service> ServicePriceTypeNavigations { get; set; }
    }
}
