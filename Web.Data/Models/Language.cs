using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class Language
    {
        public Language()
        {
            DynamicFieldAlternativeMls = new HashSet<DynamicFieldAlternativeMl>();
            DynamicFieldMls = new HashSet<DynamicFieldMl>();
            ServiceTypes = new HashSet<ServiceType>();
        }

        public string Id { get; set; }
        public string LanguageLabel { get; set; }

        public virtual ICollection<DynamicFieldAlternativeMl> DynamicFieldAlternativeMls { get; set; }
        public virtual ICollection<DynamicFieldMl> DynamicFieldMls { get; set; }
        public virtual ICollection<ServiceType> ServiceTypes { get; set; }
    }
}
