using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class ServiceLine
    {
        public ServiceLine()
        {
            ClinicalHours = new HashSet<ClinicalHour>();
        }

        public int ServiceLineId { get; set; }
        public string ServiceName { get; set; }
        public int? ServiceType { get; set; }
        public int DepartmentIdFk { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<ClinicalHour> ClinicalHours { get; set; }
    }
}
