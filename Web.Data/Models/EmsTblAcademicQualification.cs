﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Web.DLL.Models
{
    [Table("ems_tbl_academic_qualification")]
    public partial class EmsTblAcademicQualification
    {
        [Key]
        [Column("etaq_aq_id")]
        public int EtaqAqId { get; set; }
        [Required]
        [Column("etaq_qualification")]
        [StringLength(100)]
        public string EtaqQualification { get; set; }
        [Column("etaq_passing_year", TypeName = "datetime")]
        public DateTime EtaqPassingYear { get; set; }
        [Column("etaq_cgpa")]
        public double EtaqCgpa { get; set; }
        [Required]
        [Column("etaq_institute_name")]
        [StringLength(100)]
        public string EtaqInstituteName { get; set; }
        [Required]
        [Column("etaq_upload_documents")]
        public byte[] EtaqUploadDocuments { get; set; }
        [Column("eted_employee_id")]
        public int EtedEmployeeId { get; set; }
        [Required]
        [Column("etaq_created_by")]
        [StringLength(100)]
        public string EtaqCreatedBy { get; set; }
        [Required]
        [Column("etaq_created_by_name")]
        [StringLength(100)]
        public string EtaqCreatedByName { get; set; }
        [Column("etaq_created_by_date", TypeName = "datetime")]
        public DateTime EtaqCreatedByDate { get; set; }
        [Required]
        [Column("etaq_modified_by")]
        [StringLength(100)]
        public string EtaqModifiedBy { get; set; }
        [Required]
        [Column("etaq_modified_by_name")]
        [StringLength(100)]
        public string EtaqModifiedByName { get; set; }
        [Column("etaq_modified_by_date", TypeName = "datetime")]
        public DateTime EtaqModifiedByDate { get; set; }
        [Required]
        [Column("etaq_is_delete")]
        [StringLength(100)]
        public string EtaqIsDelete { get; set; }

        [ForeignKey(nameof(EtedEmployeeId))]
        [InverseProperty(nameof(EmsTblEmployeeDetails.EmsTblAcademicQualification))]
        public virtual EmsTblEmployeeDetails EtedEmployee { get; set; }
    }
}