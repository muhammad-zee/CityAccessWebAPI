using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class ConsultField
    {
        public ConsultField()
        {
            OrganizationConsultFields = new HashSet<OrganizationConsultField>();
        }

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

        public virtual ICollection<OrganizationConsultField> OrganizationConsultFields { get; set; }
    }
}
