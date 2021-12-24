using System;

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
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan StartBreak { get; set; }
        public TimeSpan EndBreak { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ServiceLine ServicelineIdFkNavigation { get; set; }
    }
}
