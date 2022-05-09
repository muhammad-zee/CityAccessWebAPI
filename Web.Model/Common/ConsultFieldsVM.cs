using System;

namespace Web.Model.Common
{
    public class ConsultFieldsVM
    {
        public int ConsultFieldId { get; set; }
        public string FieldLabel { get; set; }
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string FieldDataType { get; set; }
        public int? FieldData { get; set; }
        public int? FieldDataLength { get; set; }
        public bool? IsRequired { get; set; }
        public bool IsShowInTable { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public int SortOrder { get; set; }
        public bool IsSelected { get; set; }

        public bool showAllConsults { get; set; } = false;
        public bool IsFromDashboard { get; set; } = false;
        public string ServiceLineIds { get; set; }
        public string DepartmentIds { get; set; }
        public int OrganizationId { get; set; }

        public int UserId { get; set; }


    }
}
