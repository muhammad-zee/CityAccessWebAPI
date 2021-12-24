using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class Department
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int? OrganizationIdFk { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
