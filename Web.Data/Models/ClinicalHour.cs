﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class ClinicalHour
    {
        public int ClinicalHourId { get; set; }
        public int WeekDayIdFk { get; set; }
        public int? OrganizationIdFk { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Organization OrganizationIdFkNavigation { get; set; }
    }
}
