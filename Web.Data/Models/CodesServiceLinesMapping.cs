#nullable disable

namespace Web.Data.Models
{
    public partial class CodesServiceLinesMapping
    {
        public int CodesServiceLinesMappingId { get; set; }
        public int OrganizationIdFk { get; set; }
        public int CodeIdFk { get; set; }
        public int DefaultServiceLineIdFk { get; set; }
        public int? ServiceLineId1Fk { get; set; }
        public int? ServiceLineId2Fk { get; set; }
        public int ActiveCodeId { get; set; }
        public string ActiveCodeName { get; set; }
    }
}
