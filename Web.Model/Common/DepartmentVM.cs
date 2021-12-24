using System;
using System.Collections.Generic;

namespace Web.Model.Common
{
    public class DepartmentVM
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public int? OrganizationIdFk { get; set; }

        public string ServicesIdFks { get; set; }
        public List<ServiceLineVM> ServiceLines { get; set; }

    }
}
