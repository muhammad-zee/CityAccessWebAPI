using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class ConsultFieldsVM
    {
        public int ConsultFieldId { get; set; }
        public string FieldName { get; set; }
        public int FieldType { get; set; }
        public int FieldDataType { get; set; }
        public int? FieldDataLength { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
