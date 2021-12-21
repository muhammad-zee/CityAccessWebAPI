using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class IvrTreeVM
    {
        public string key { get; set; }

        public string label { get; set; }
        public string data { get; set; }
        public string expandedIcon { get; set; }
        public string collapsedIcon { get; set; }
        public int? ParentKey { get; set; }
        public int? KeyPress { get; set; }
        public bool expanded { get;set;}
        public int? organizationTypeIdFk { get; set; }
        public IList<IvrTreeVM> children { get; set; }
        
    }
   
}
