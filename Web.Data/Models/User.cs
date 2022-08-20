using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class User
    {
        public User()
        {
            AgreementLogs = new HashSet<AgreementLog>();
            ErrorLogs = new HashSet<ErrorLog>();
            RequestBookers = new HashSet<Request>();
            RequestLogs = new HashSet<RequestLog>();
            RequestResponsibles = new HashSet<Request>();
            UserLogEditors = new HashSet<UserLog>();
            UserLogUsers = new HashSet<UserLog>();
        }

        public int Id { get; set; }
        public Guid? UserIcalLink { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public int PartnerId { get; set; }
        public bool? IsActive { get; set; }
        public bool? EmailConfirmed { get; set; }
        public bool? IsAdmin { get; set; }

        public virtual Partner Partner { get; set; }
        public virtual ICollection<AgreementLog> AgreementLogs { get; set; }
        public virtual ICollection<ErrorLog> ErrorLogs { get; set; }
        public virtual ICollection<Request> RequestBookers { get; set; }
        public virtual ICollection<RequestLog> RequestLogs { get; set; }
        public virtual ICollection<Request> RequestResponsibles { get; set; }
        public virtual ICollection<UserLog> UserLogEditors { get; set; }
        public virtual ICollection<UserLog> UserLogUsers { get; set; }
    }
}
