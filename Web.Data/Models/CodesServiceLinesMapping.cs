#nullable disable

namespace Web.Data.Models
{
    public partial class CodesServiceLinesMapping
    {
        public int CodesServiceLinesMappingId { get; set; }
        public int OrganizationIdFk { get; set; }
        public int CodeIdFk { get; set; }
        public int ServiceLineIdFk { get; set; }
        public int ActiveCodeId { get; set; }
        public string ActiveCodeName { get; set; }
    }
}
