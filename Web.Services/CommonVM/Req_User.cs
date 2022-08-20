using System;
using Web.Data.Models;

namespace Web.Services.CommonVM
{
    public class Req_User
    {
        public Req_forTransfer req_ForTransfer { get; set; }
        public Request Request { get; set; }
        public User User { get; set; }
        public Agreement Agreement { get; set; }
        public Partner Partner { get; set; }
        public string Status { get; set; }
        public ServiceImage serviceImage { get; set; }


        //Agreement and Service common fields
        public string Description { get; set; }
        public string CommissionType { get; set; }
        public string PriceType { get; set; }
        public string AgentPaymentType { get; set; }
        public string AgentInstructions { get; set; }
        public string ConfirmationText { get; set; }
        public string CancellationPolicy { get; set; }
        public Nullable<decimal> CommissionValue { get; set; }
        public Nullable<decimal> PriceValue { get; set; }
        public Nullable<decimal> AgentPaymentValue { get; set; }
        public string AgName { get; set; }
        public Nullable<bool> isConfirmed { get; set; }


        public Nullable<bool> FromPartnerSite { get; set; }
    }
}