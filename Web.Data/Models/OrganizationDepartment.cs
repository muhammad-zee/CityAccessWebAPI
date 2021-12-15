using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class OrganizationDepartment
    {
        public int OrganizationDepartmentId { get; set; }
        public int OrganizationIdFk { get; set; }
        public int DepartmentIdFk { get; set; }

        public virtual Department DepartmentIdFkNavigation { get; set; }
        public virtual Organization OrganizationIdFkNavigation { get; set; }
    }
}
