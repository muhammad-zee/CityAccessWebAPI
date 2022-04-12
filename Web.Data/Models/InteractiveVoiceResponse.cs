using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class InteractiveVoiceResponse
    {
        public InteractiveVoiceResponse()
        {
            Ivrsettings = new HashSet<Ivrsetting>();
        }

        public int IvrId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OrganizationIdFk { get; set; }
        public int ServicelineIdFk { get; set; }
        public string LandlineNumber { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Ivrsetting> Ivrsettings { get; set; }
    }
}
