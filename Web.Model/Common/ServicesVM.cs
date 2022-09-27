using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class ServicesVM
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public string  ServiceName { get; set; }
        public string Descritpion { get; set; }
        public TimeSpan Duration { get; set; }
        public int? CommissionTypeId { get; set; }
        public string CommissionTypeName { get; set; }
        public int? MaxNumberOfPersons { get; set; }
        public int? MinNumberOfPersons { get; set; }
        public int? Availability { get; set; }
        public decimal? Price { get; set; }
        public decimal? TypeofPrice { get; set; }
        public bool? OverridePrice { get; set; }
        public decimal? AgentPayment { get; set; }
        public decimal? TypeOfAgentPayment { get; set; }
        public string AgentInstructions { get; set; }
        public string ConfirmationText { get; set; }
        public string CancellationPolicy { get; set; }
        public string City { get; set; }
        public bool IsPublic { get; set; }
        public bool? IsActive { get; set; }
        public string ServiceImage { get; set; }

        public string PartnerTradeName { get; set; }










    }
}
