using System.ComponentModel.DataAnnotations.Schema;

namespace Web.Data.Models
{
    [NotMapped]
    public class ScheduleListFomSP
    {
        public int Id { get; set; }
        public int RoleIdFk { get; set; }
        public int UserIdFk { get; set; }
        public int? ServiceLineIdFk { get; set; }
        public int? DateRangeId { get; set; }
        public int? OrganizationIdFk { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}
