using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class ComponentAccessByRoleAndUserTreeVM
    {
        public string ComponentId { get; set; }
        public string ModuleName { get; set; }
        public int? ParentComponentId { get; set; }
        public List<string> Actions { get; set; }

        [JsonIgnore]
        public List<ComponentAccessByRoleAndUserTreeVM> children { get; set; }
        [JsonIgnore]
        public bool IsAction { get; set; }

    }
 
}
