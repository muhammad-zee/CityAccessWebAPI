using System;

#nullable disable

namespace Web.Data.Models
{
    public partial class State
    {
        public int StateId { get; set; }
        public string StateName { get; set; }
        public string StateProvince { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
