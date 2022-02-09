using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class ActiveCodeVM
    {
        public int ActiveCodeId { get; set; }
        public string ActiveCodeName { get; set; }
        public int OrganizationIdFk { get; set; }
        public string OrganizationName { get; set; }
        public int CodeIdFk { get; set; }
        public string ServiceLineIds { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public List<ServiceLineVM> serviceLines { get; set; }
    }
}
