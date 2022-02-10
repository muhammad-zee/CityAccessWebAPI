using System;

namespace Web.Model.Common
{
    public class CodeTrumaVM
    {
        public int CodeTraumaId { get; set; }
        public string PatientName { get; set; }
        public DateTime? Dob { get; set; }
        public int? Gender { get; set; }
        public string GenderTitle { get; set; }
        public string ChiefComplant { get; set; }
        public DateTime? LastKnownWell { get; set; }
        public string Hpi { get; set; }
        public int? BloodThinners { get; set; }
        public string BloodThinnersTitle { get; set; }
        public string FamilyContactName { get; set; }
        public string FamilyContactNumber { get; set; }
        public bool? IsEms { get; set; }
        public bool? IsCompleted { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
