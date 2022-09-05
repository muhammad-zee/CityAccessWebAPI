using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class AgreementsFilterVM
    {
        public int AgentId { get; set; }
        public int OperatorId { get; set; }
        public string SearchString { get; set; }
        public int ServiceId { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
