using System;
using System.ComponentModel.DataAnnotations;

namespace Web.Model.Common
{
    public class RegisterCredentialVM
    {
        public int UserId { get; set; }
        [Required]
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        [Required]
        public string PrimaryEmail { get; set; }
        public string SecondaryEmail { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string OfficePhoneNumber { get; set; }
        public string PersonalMobileNumber { get; set; }
        [Required]
        public string Gender { get; set; }
        public string OfficeAddress { get; set; }
        public string HomeAddress { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string City { get; set; }
        public int? StateKey { get; set; }
        public string Zip { get; set; }
        public byte[] UserImageByte { get; set; }
        public string UserImage { get; set; }
        public string RoleIds { get; set; }
    }
}
