using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class Partner
    {
        public Partner()
        {
            Agreements = new HashSet<Agreement>();
            PartnerLogos = new HashSet<PartnerLogo>();
            Services = new HashSet<Service>();
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public Guid? IcalLink { get; set; }
        public string TradeName { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public string FiscalId { get; set; }
        public string CountryId { get; set; }
        public string InvoiceName { get; set; }
        public string InvoiceAddress { get; set; }
        public bool? IsAgent { get; set; }
        public bool? IsOperator { get; set; }
        public bool? IsActive { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public bool? IsPublic { get; set; }
        public int? Invitedby { get; set; }
        public bool? IsTest { get; set; }

        public virtual ICollection<Agreement> Agreements { get; set; }
        public virtual ICollection<PartnerLogo> PartnerLogos { get; set; }
        public virtual ICollection<Service> Services { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
