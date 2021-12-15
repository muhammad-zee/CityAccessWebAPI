using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class DepartmentService
    {
        public int DepartmentServiceId { get; set; }
        public int ServiceIdFk { get; set; }
        public int DepartmentIdFk { get; set; }

        public virtual Department DepartmentIdFkNavigation { get; set; }
        public virtual ServiceLine ServiceIdFkNavigation { get; set; }
    }
}
