using System;

namespace Web.Model.Common
{
    public class InhouseCodeFeildsVM
    {
        public int InhouseCodesFieldId { get; set; }
        public string FieldLabel { get; set; }
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string FieldDataType { get; set; }
        public int? FieldDataLength { get; set; }
        public int? FieldData { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsShowInTable { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public bool IsSelected { get; set; }

    }
}
