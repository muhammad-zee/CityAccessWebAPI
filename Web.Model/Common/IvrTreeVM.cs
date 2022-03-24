using System.Collections.Generic;

namespace Web.Model.Common
{
    public class IvrTreeVM
    {
        public string key { get; set; }
        public string label { get; set; }
        public string data { get; set; }
        public string icon { get; set; }
        public string expandedIcon { get; set; }
        public string collapsedIcon { get; set; }
        public int? ParentKey { get; set; }
        public int? KeyPress { get; set; }
        public int? NodeTypeId { get; set; }
        public bool expanded { get; set; }
        public IList<IvrTreeVM> children { get; set; }
    }

}
