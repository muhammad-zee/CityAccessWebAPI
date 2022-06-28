using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class DeleteChannelsFilterVM
    {

        public string OrganizationIds { get; set; }
        public string UserIds { get; set; }
        public bool CustomDateRange { get; set; }
        public bool GroupConversations { get; set; }
        public bool SingleConversations { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

    }
}
