using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class ServiceLine
    {
        public ServiceLine()
        {
            ClinicalHolidays = new HashSet<ClinicalHoliday>();
            ClinicalHours = new HashSet<ClinicalHour>();
            UsersRelations = new HashSet<UsersRelation>();
            UsersSchedules = new HashSet<UsersSchedule>();
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

        public virtual Department DepartmentIdFkNavigation { get; set; }
        public virtual ICollection<ClinicalHoliday> ClinicalHolidays { get; set; }
        public virtual ICollection<ClinicalHour> ClinicalHours { get; set; }
        public virtual ICollection<UsersRelation> UsersRelations { get; set; }
        public virtual ICollection<UsersSchedule> UsersSchedules { get; set; }
    }
}
