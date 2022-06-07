﻿using System;
using System.Collections.Generic;

namespace Web.Model.Common
{
    public class CodeStrokeVM
    {
        public int Id { get; set; }
        public int CodeStrokeId { get; set; }
        public long CodeNumber { get; set; }
        public long CodeStrokeNumber { get; set; }
        public string PatientName { get; set; }
        public DateTime? Dob { get; set; }
        public string DobStr { get; set; }
        public int? Gender { get; set; }
        public int OrganizationIdFk { get; set; }
        public string GenderTitle { get; set; }
        public string ChiefComplant { get; set; }
        public string LastKnownWellStr { get; set; }
        public DateTime? LastKnownWell { get; set; }
        public string Hpi { get; set; }
        public string BloodThinners { get; set; }
        public List<object> BloodThinnersTitle { get; set; }
        public string FamilyContactName { get; set; }
        public string FamilyContactNumber { get; set; }
        public bool? IsEms { get; set; }
        public bool? IsCompleted { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedDateStr { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? EndTime { get; set; }
        public string EstimatedTime { get; set; }
        public TimeSpan? ActualTime { get; set; }
        public string Distance { get; set; }
        public string StartingPoint { get; set; }
        public List<FilesVM> Audios { get; set; }
        public List<FilesVM> Videos { get; set; }
        public List<FilesVM> Attachment { get; set; }
        public int DefaultServiceLineId { get; set; }
        public string SelectedServiceLineIds { get; set; }
        public ServiceLineVM DefaultServiceLine { get; set; }
        public List<ServiceLineVM> ServiceLines { get; set; }

        public string DefaultServiceLineIds { get; set; }
        public string ServiceLineTeam1Ids { get; set; }
        public string ServiceLineTeam2Ids { get; set; }

        public List<ServiceLineVM> DefaultServiceLineTeam { get; set; }
        public List<ServiceLineVM> ServiceLineTeam1 { get; set; }
        public List<ServiceLineVM> ServiceLineTeam2 { get; set; }


        public string Audio { get; set; }
        public string Video { get; set; }
        public string Attachments { get; set; }
        public string CodeName { get; set; }

        public List<string> AudiosPath { get; set; } = new List<string>();
        public List<string> VideosPath { get; set; } = new List<string>();
        public List<string> AttachmentsPath { get; set; } = new List<string>();
        public object OrganizationData { get; set; }
        public string AudioFolderRoot { get; set; }
        public string VideoFolderRoot { get; set; }
        public string AttachmentsFolderRoot { get; set; }

        public string ChannelSid { get; set; }
        public string UserUniqueId { get; set; }
        public int UserIdFk { get; set; }
        public string FieldName { get; set; }
        public string FieldDataType { get; set; }

    }
}
