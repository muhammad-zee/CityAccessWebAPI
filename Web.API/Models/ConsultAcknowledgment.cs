﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class ConsultAcknowledgment
    {
        public int ConsultAcknowledgmentId { get; set; }
        public long ConsultIdFk { get; set; }
        public int UserIdFk { get; set; }
        public bool IsAcknowledge { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Consult ConsultIdFkNavigation { get; set; }
    }
}
