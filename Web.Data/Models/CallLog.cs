using System;

#nullable disable

namespace Web.Data.Models
{
    public partial class CallLog
    {
        public int Id { get; set; }
        public DateTime? StartTime { get; set; }
        public string Duration { get; set; }
        public string Type { get; set; }
        public string Direction { get; set; }
        public string Action { get; set; }
        public string Result { get; set; }
        public string TophoneNumber { get; set; }
        public string Toname { get; set; }
        public string FromphoneNumber { get; set; }
        public string Fromname { get; set; }
        public string Fromlocation { get; set; }
        public string Reason { get; set; }
        public string ReasonDescription { get; set; }
        public string RecordingUri { get; set; }
        public string RecordingType { get; set; }
        public string RecordingContentUri { get; set; }
        public DateTime? Cd { get; set; }
        public bool? Taskgenerated { get; set; }
        public string RecordingName { get; set; }
        public string Isrecorded { get; set; }
        public string Callsource { get; set; }
        public string ConferenceName { get; set; }
        public string RecordingDateTime { get; set; }
        public string PatientId { get; set; }
        public string ParentCallSid { get; set; }
    }
}
