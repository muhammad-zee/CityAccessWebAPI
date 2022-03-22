﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class ConsultVM
    {
        public bool showAllConsults { get; set; } = false;
        public bool IsFromDashboard { get; set; } = false;
        public string ServiceLineIds { get; set; }
        public string DepartmentIds { get; set; }
        public int OrganizationId { get; set; }

        public int UserId { get; set; }

        public int PageNumber { get; set; }
        public int Rows { get; set; }
        public string Filter { get; set; }
        public string sortOrder { get; set; }
        public string SortCol { get; set; }
        public string FilterVal { get; set; }
    }
}
