namespace Web.Model.Common
{
    public class ScheduleListVM
    {
        public int Id { get; set; }
        public int RoleIdFk { get; set; }
        public int UserIdFk { get; set; }
        public int? ServiceLineIdFk { get; set; }
        public int? DateRangeId { get; set; }
        public int? OrganizationIdFk { get; set; }
        public string UserName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public string ServiceName { get; set; }
        public int Total_Records { get; set; }

        public string Initials { get; set; }
        public string DepartmentName { get; set; }
    }
}
