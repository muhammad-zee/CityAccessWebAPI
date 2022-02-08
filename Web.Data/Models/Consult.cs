﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class Consult
    {
        public int ConsultId { get; set; }
        public long ConsultNumber { get; set; }
        public int ServiceLineIdFk { get; set; }
        public bool IsNewPatient { get; set; }
        public string PatientFirstName { get; set; }
        public string PatientLastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string MedicalRecordNumber { get; set; }
        public bool IsCallbackRequired { get; set; }
        public int? CallbackNumber { get; set; }
        public string Location { get; set; }
        public string ConsultType { get; set; }
        public string ConsultReason { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ServiceLine ServiceLineIdFkNavigation { get; set; }
    }
}
