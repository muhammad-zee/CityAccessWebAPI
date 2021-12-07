namespace Web.Model.Common
{
    public class ComponentAccessVM
    {
        public int ComponentId { get; set; }
        public string ComModuleName { get; set; }
        public object RoleId { get; set; }
        public int? ParentComponentId { get; set; }
        public bool IsActive { get; set; }
        public bool IsAction { get; set; }
    }


}
