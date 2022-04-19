using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class Department
    {
        public Department()
        {
            ServiceLines = new HashSet<ServiceLine>();
        }

        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int? OrganizationIdFk { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Organization OrganizationIdFkNavigation { get; set; }
        public virtual ICollection<ServiceLine> ServiceLines { get; set; }
    }
}
