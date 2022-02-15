﻿using System;
using System.Collections.Generic;

namespace Web.Model.Common
{
    public class CodeSTEMIVM
    {
        public int CodeStemiid { get; set; }
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
        public int OrganizationIdFk { get; set; }
        public List<FilesVM> Audios { get; set; }
        public List<FilesVM> Videos { get; set; }
        public List<FilesVM> Attachment { get; set; }

        public string Audio { get; set; }
        public string Video { get; set; }
        public string Attachments { get; set; }

        public List<string> AudiosPath { get; set; }
        public List<string> VideosPath { get; set; }
        public List<string> AttachmentsPath { get; set; }

        public string AudioFolderRoot { get; set; }
        public string VideoFolderRoot { get; set; }
        public string AttachmentsFolderRoot { get; set; }
    }
}
