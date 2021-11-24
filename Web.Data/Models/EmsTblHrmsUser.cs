﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Web.DLL.Models
{
    [Table("ems_tbl_hrms_user")]
    public partial class EmsTblHrmsUser
    {
        [Key]
        [Column("ethu_user_id")]
        public int EthuUserId { get; set; }
        [Required]
        [Column("ethu_full_name")]
        [StringLength(100)]
        public string EthuFullName { get; set; }
        [Required]
        [Column("ethu_user_name")]
        [StringLength(100)]
        public string EthuUserName { get; set; }
        [Required]
        [Column("ethu_email_address")]
        [StringLength(100)]
        public string EthuEmailAddress { get; set; }
        [Required]
        [Column("ethu_phone_number")]
        [StringLength(100)]
        public string EthuPhoneNumber { get; set; }
        [Required]
        [Column("ethu_password")]
        [StringLength(100)]
        public string EthuPassword { get; set; }
        [Required]
        [Column("ethu_gender")]
        [StringLength(100)]
        public string EthuGender { get; set; }
        [Required]
        [Column("ethu_created_by")]
        [StringLength(100)]
        public string EthuCreatedBy { get; set; }
        [Required]
        [Column("ethu_created_by_name")]
        [StringLength(100)]
        public string EthuCreatedByName { get; set; }
        [Column("ethu_created_by_date", TypeName = "datetime")]
        public DateTime EthuCreatedByDate { get; set; }
        [Required]
        [Column("ethu_modified_by")]
        [StringLength(100)]
        public string EthuModifiedBy { get; set; }
        [Required]
        [Column("ethu_modified_by_name")]
        [StringLength(100)]
        public string EthuModifiedByName { get; set; }
        [Column("ethu_modified_by_date", TypeName = "datetime")]
        public DateTime EthuModifiedByDate { get; set; }
        [Required]
        [Column("ethu_is_delete")]
        [StringLength(100)]
        public string EthuIsDelete { get; set; }
    }
}