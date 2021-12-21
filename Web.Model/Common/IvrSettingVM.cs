using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class IvrSettingVM
    {
        public int IvrId { get; set; }
        public int? IvrparentId { get; set; }
        public int? OrganizationTypeIdFk { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? KeyPress { get; set; }
        public string Icon { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
