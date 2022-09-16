using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class PartnerVM
    {
        public int PartnerId { get; set; }
        public string TradeName { get; set; }
        public string Description { get; set; }
        public string ContactPerson { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string NotificationEmail { get; set; }
        public string InvoiceName { get; set; }
        public string VatNumber { get; set; }
        public string InvoiceAddress { get; set; }
        public bool IsAgent { get; set; }
        public bool IsOperator { get; set; }
        public bool IsActive { get; set; }
        public bool IsPublic { get; set; }
        public string Country { get; set; }
        public string Logo { get; set; }

    }
}
