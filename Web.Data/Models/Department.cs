using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class Department
    {
        public Department()
        {
            DepartmentServices = new HashSet<DepartmentService>();
            OrganizationDepartments = new HashSet<OrganizationDepartment>();
        }

        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsDeleted { get; set; }

        public virtual ICollection<DepartmentService> DepartmentServices { get; set; }
        public virtual ICollection<OrganizationDepartment> OrganizationDepartments { get; set; }
    }
}
