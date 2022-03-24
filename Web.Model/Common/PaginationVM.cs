using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class PaginationVM
    {
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
