using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class DynamicFieldMl
    {
        public string LanguageId { get; set; }
        public string Label { get; set; }
        public int DynamicfieldId { get; set; }

        public virtual DynamicField Dynamicfield { get; set; }
        public virtual Language Language { get; set; }
    }
}
