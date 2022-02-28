using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class ActiveCode
    {
        public int ActiveCodeId { get; set; }
        public int OrganizationIdFk { get; set; }
        public int CodeIdFk { get; set; }
        public string ServiceLineIds { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Organization OrganizationIdFkNavigation { get; set; }
    }
}
