using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class User
    {
        public User()
        {
            ConversationParticipants = new HashSet<ConversationParticipant>();
            UserAccesses = new HashSet<UserAccess>();
            UserRoles = new HashSet<UserRole>();
            UsersRelations = new HashSet<UsersRelation>();
            UsersSchedules = new HashSet<UsersSchedule>();
        }

        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Initials { get; set; }
        public string PrimaryEmail { get; set; }
        public string SecondaryEmail { get; set; }
        public string Password { get; set; }
        public string OfficePhoneNumber { get; set; }
        public string PersonalMobileNumber { get; set; }
        public string Gender { get; set; }
        public string OfficeAddress { get; set; }
        public string HomeAddress { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string City { get; set; }
        public int? StateKey { get; set; }
        public string Zip { get; set; }
        public string UserImage { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string TwoFactorCode { get; set; }
        public DateTime? CodeExpiryTime { get; set; }
        public bool IsTwoFactRememberChecked { get; set; }
        public DateTime? TwoFactorExpiryDate { get; set; }
        public bool IsRequirePasswordReset { get; set; }
        public bool? IsActive { get; set; }
        public string UserChannelSid { get; set; }
        public string UserUniqueId { get; set; }

        public virtual ICollection<ConversationParticipant> ConversationParticipants { get; set; }
        public virtual ICollection<UserAccess> UserAccesses { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<UsersRelation> UsersRelations { get; set; }
        public virtual ICollection<UsersSchedule> UsersSchedules { get; set; }
    }
}
