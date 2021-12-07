using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class ComponentAccessTreeVM
    {
        public string ComponentId { get; set; }
        public string ModuleName { get; set; }
        public int? ParentComponentId { get; set; }
        public List<string> Actions { get; set; }

        [JsonIgnore]
        public List<ComponentAccessTreeVM> children { get; set; }

    }
    public class ComponentAccessDbReturnVM
    {
        public int ComponentId { get; set; }
        public string ComModuleName { get; set; }
        public int? ParentComponentId { get; set; }
        public bool IsActive { get; set; }

    }
}
