using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class DynamicField
    {
        public DynamicField()
        {
            DynamicFieldAlternatives = new HashSet<DynamicFieldAlternative>();
            DynamicFieldMls = new HashSet<DynamicFieldMl>();
        }

        public int Id { get; set; }
        public string FieldType { get; set; }

        public virtual ICollection<DynamicFieldAlternative> DynamicFieldAlternatives { get; set; }
        public virtual ICollection<DynamicFieldMl> DynamicFieldMls { get; set; }
    }
}
