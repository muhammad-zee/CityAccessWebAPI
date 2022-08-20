using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class ServiceImage
    {
        public int Id { get; set; }
        public int? ServiceId { get; set; }
        public byte[] Image { get; set; }
        public int? SequenceNr { get; set; }

        public virtual Service Service { get; set; }
    }
}
