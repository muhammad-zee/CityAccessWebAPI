using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class EmailLog
    {
        public int Id { get; set; }
        public string Form { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool? IsMailSent { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime SentDate { get; set; }
        public int SentBy { get; set; }
    }
}
