using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class ComponentVM
    {
        public int ComponentId { get; set; }
        public string ComModuleName { get; set; }
        public int? ParentComponentId { get; set; }
        public int RoleIdFk { get; set; }
        public string PageUrl { get; set; }
        public string PageName { get; set; }
        public string PageTitle { get; set; }
        public string PageDescription { get; set; }
        public string FormId { get; set; }
        public bool Status { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public int? SortOrder { get; set; }
        public string ModuleImage { get; set; }
    }
}
