using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class ClinicalHoliday
    {
        public int ClinicalHolidayId { get; set; }
        public int ServicelineIdFk { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ServiceLine ServicelineIdFkNavigation { get; set; }
    }
}
