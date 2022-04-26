using System;

namespace Web.Model.Common
{
    public class OrgCodeStrokeFeildsVM
    {
        public int OrgCodeStrokeFieldId { get; set; }
        public int OrganizationIdFk { get; set; }
        public int InhouseCodesFieldIdFk { get; set; }
        public int? SortOrder { get; set; }
        public bool IsRequired { get; set; }
        public bool IsShowInTable { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public string CodeName { get; set; }
    }
}
