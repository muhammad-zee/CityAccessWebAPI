using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class CommissionType
    {
        public CommissionType()
        {
            Agreements = new HashSet<Agreement>();
        }

        public int Id { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Agreement> Agreements { get; set; }
    }
}
