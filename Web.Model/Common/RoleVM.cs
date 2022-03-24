using System;

namespace Web.Model.Common
{
    public class RoleVM
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
        public string RoleDiscrimination { get; set; }

        public int OrganizationIdFk { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsScheduleRequired { get; set; }

        ///// For Get All Users Pagination
        public int OrganizationId { get; set; }
        public int Total_Records { get; set; }
        public int PageNumber { get; set; }
        public int Rows { get; set; }
        public string Filter { get; set; }
        public string SortOrder { get; set; }
        public string SortCol { get; set; }
        public string FilterVal { get; set; }
    }

   
}
