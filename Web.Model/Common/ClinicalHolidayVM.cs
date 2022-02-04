using System;
using System.Collections.Generic;

namespace Web.Model.Common
{
    public class ClinicalHolidayVM
    {
        public int ClinicalHolidayId { get; set; }
        public int ServicelineIdFk { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<DateTime> SelectiveDates { get; set; }
        public List<string> SelectedDateStr { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
