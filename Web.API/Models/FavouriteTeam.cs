using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class FavouriteTeam
    {
        public int FavouriteTeamId { get; set; }
        public int UserIdFk { get; set; }
        public int ServiceLineIdFk { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ServiceLine ServiceLineIdFkNavigation { get; set; }
        public virtual User UserIdFkNavigation { get; set; }
    }
}
