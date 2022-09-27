using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class DynamicFieldAlternativeVM
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public int DynamicFieldId { get; set; }
        public string DynamicFieldName { get; set; }
    }
}
