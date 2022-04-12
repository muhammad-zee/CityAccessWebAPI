using System;

namespace Web.Model.Common
{
    public class IvrSettingVM
    {
        public int IvrSettingsId { get; set; }
        public int IvrIdFk { get; set; }
        public int? IvrparentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int NodeTypeId { get; set; }
        public int? KeyPress { get; set; }
        public int? EnqueueToRoleIdFk { get; set; }
        public string Icon { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
