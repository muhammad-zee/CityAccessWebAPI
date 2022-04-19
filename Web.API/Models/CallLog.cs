using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class CallLog
    {
        public int CallLogId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Duration { get; set; }
        public string Direction { get; set; }
        public string CallStatus { get; set; }
        public string ToPhoneNumber { get; set; }
        public string ToName { get; set; }
        public string FromPhoneNumber { get; set; }
        public string FromName { get; set; }
        public string CallSid { get; set; }
        public string ParentCallSid { get; set; }
        public string RecordingName { get; set; }
        public bool IsRecorded { get; set; }
        public bool IsAppToAppCall { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
