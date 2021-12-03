using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class TreeviewItemVM
    {
        public bool? @checked { get; set; }
        public List<TreeviewItemVM> children { get; set; }
        public bool? expanded { get; set; } = true;
        public bool? disabled { get; set; }
        public string label { get; set; }
        public string key { get; set; }
        public int? ParentKey { get; set; }


        //public TreeviewItemVM()
        //{
        //    this.@checked = false;
        //    this.disabled = false;
        //}
    }
   
}
