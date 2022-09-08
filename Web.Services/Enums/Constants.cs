using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Services.Enums
{
    public static class Constants
    {
        public static class StateTransitions
        {
            public const string Approved = "Approved";
            public const string Canceled = "Canceled";
            public const string Invoiced = "Invoiced";
            public const string SiteApproval = "Site Approval";
            public const string SiteCanceled = "Site Canceled";
            public const string Submitted = "Submitted";
        }
    }
}
