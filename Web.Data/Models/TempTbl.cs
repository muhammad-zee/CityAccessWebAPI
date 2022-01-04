using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class TempTbl
    {
        public int UsersScheduleId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
