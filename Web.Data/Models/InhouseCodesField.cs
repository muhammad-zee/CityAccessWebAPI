using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class InhouseCodesField
    {
        public InhouseCodesField()
        {
            OrganizationCodeBlueFields = new HashSet<OrganizationCodeBlueField>();
            OrganizationCodeSepsisFields = new HashSet<OrganizationCodeSepsisField>();
            OrganizationCodeStemifields = new HashSet<OrganizationCodeStemifield>();
            OrganizationCodeStrokeFields = new HashSet<OrganizationCodeStrokeField>();
            OrganizationCodeTraumaFields = new HashSet<OrganizationCodeTraumaField>();
        }

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
        public bool IsRequried { get; set; }
        public bool IsDeleted { get; set; }
        public bool ForStrokeEms { get; set; }
        public bool ForSepsisEms { get; set; }
        public bool ForStemiems { get; set; }
        public bool ForTraumaEms { get; set; }
        public bool ForBlueEms { get; set; }

        public virtual ICollection<OrganizationCodeBlueField> OrganizationCodeBlueFields { get; set; }
        public virtual ICollection<OrganizationCodeSepsisField> OrganizationCodeSepsisFields { get; set; }
        public virtual ICollection<OrganizationCodeStemifield> OrganizationCodeStemifields { get; set; }
        public virtual ICollection<OrganizationCodeStrokeField> OrganizationCodeStrokeFields { get; set; }
        public virtual ICollection<OrganizationCodeTraumaField> OrganizationCodeTraumaFields { get; set; }
    }
}
