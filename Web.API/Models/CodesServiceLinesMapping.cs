using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class CodesServiceLinesMapping
    {
        public int CodesServiceLinesMappingId { get; set; }
        public int OrganizationIdFk { get; set; }
        public int CodeIdFk { get; set; }
        public string DefaultServiceLineIdFk { get; set; }
        public string ServiceLineId1Fk { get; set; }
        public string ServiceLineId2Fk { get; set; }
        public int ActiveCodeId { get; set; }
        public string ActiveCodeName { get; set; }
    }
}
