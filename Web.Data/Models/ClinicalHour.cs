using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class ClinicalHour
    {
        public int ClinicalHourId { get; set; }
        public int WeekDayIdFk { get; set; }
        public int ServicelineIdFk { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime? StartBreak { get; set; }
        public DateTime? EndBreak { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ServiceLine ServicelineIdFkNavigation { get; set; }
    }
}
