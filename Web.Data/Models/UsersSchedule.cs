using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class UsersSchedule
    {
        public int UsersScheduleId { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan ScheduleTimeStart { get; set; }
        public TimeSpan ScheduleTimeEnd { get; set; }
        public int UserIdFk { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual User UserIdFkNavigation { get; set; }
    }
}
