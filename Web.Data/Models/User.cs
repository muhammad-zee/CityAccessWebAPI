using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
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
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string City { get; set; }
        public int? StateKey { get; set; }
        public string Zip { get; set; }
        public string UserImage { get; set; }
    }
}
