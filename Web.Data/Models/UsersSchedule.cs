using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class UsersSchedule
    {
        public int UsersScheduleId { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public DateTime ScheduleDateStart { get; set; }
        public DateTime ScheduleDateEnd { get; set; }
        public int UserIdFk { get; set; }
        public int RoleIdFk { get; set; }
        public int? DateRangeId { get; set; }
        public int? ServiceLineIdFk { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
