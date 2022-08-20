using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class PartnerLogo
    {
        public int Id { get; set; }
        public int? PartnerId { get; set; }
        public byte[] Image { get; set; }

        public virtual Partner Partner { get; set; }
    }
}
