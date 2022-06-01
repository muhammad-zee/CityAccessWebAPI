using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Web.Model.Common
{
    public class ComponentAccessByRoleAndUserTreeVM
    {
        public string ComponentId { get; set; }
        public string ModuleName { get; set; }
        public int? ParentComponentId { get; set; }
        public List<object> Actions { get; set; }

        [JsonIgnore]
        public List<ComponentAccessByRoleAndUserTreeVM> children { get; set; }
        [JsonIgnore]
        public bool IsAction { get; set; }

    }

}
