using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class OrganizationConsultField
    {
        public int OrgConsultFieldId { get; set; }
        public int OrganizationIdFk { get; set; }
        public int ConsultFieldIdFk { get; set; }
        public int? SortOrder { get; set; }
        public bool IsRequired { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ConsultField ConsultFieldIdFkNavigation { get; set; }
        public virtual Organization OrganizationIdFkNavigation { get; set; }
    }
}
