using System;
using System.Collections.Generic;

namespace Web.Model.Common
{
    public class ActiveCodeVM
    {
        public int ActiveCodeId { get; set; }
        public string ActiveCodeName { get; set; }
        public int OrganizationIdFk { get; set; }
        public string OrganizationName { get; set; }
        public int CodeIdFk { get; set; }
        public string Type { get; set; }
        public string DefaultServiceLineTeam { get; set; }
        public string ServiceLineTeam1 { get; set; }
        public string ServiceLineTeam2 { get; set; }
        public string ServiceLineIds { get; set; }
        public int DefaultServiceLineId { get; set; }
        public string DefaultServiceLineName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEMS { get; set; }
        public bool showAllActiveCodes { get; set; }
        public bool fromDashboard { get; set; }
        public bool Status { get; set; }

        public List<ServiceLineVM> DefaultServiceLineTeamList { get; set; }
        public List<ServiceLineVM> ServiceLineTeam1List { get; set; }
        public List<ServiceLineVM> ServiceLineTeam2List { get; set; }

        public List<ServiceLineVM> serviceLines { get; set; }


        public int PageNumber { get; set; }
        public int Rows { get; set; }
        public string Filter { get; set; }
        public string SortOrder { get; set; }
        public string SortCol { get; set; }
        public string FilterCol { get; set; }
        public string FilterVal { get; set; }
    }
}
