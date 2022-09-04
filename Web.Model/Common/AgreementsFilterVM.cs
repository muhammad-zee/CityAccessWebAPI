using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class AgreementsFilterVM
    {
        public bool agr;

        public string Agent { get; set; }
        public string Operator1 { get; set; }
        public string SearchString { get; set; }
        public string Service { get; set; }
    }
}
