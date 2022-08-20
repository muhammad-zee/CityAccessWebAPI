using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class DynamicFieldAlternativeMl
    {
        public string LanguageId { get; set; }
        public string Label { get; set; }
        public int DynamicfieldalternativeId { get; set; }

        public virtual DynamicFieldAlternative Dynamicfieldalternative { get; set; }
        public virtual Language Language { get; set; }
    }
}
