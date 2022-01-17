using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.Model.Common
{
    public class RegisterCredentialVM
    {
        public int UserId { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string FirstName { get; set; } = "";
        public string MiddleName { get; set; } = "";
        [Required]
        public string LastName { get; set; }
        public string Initials { get; set; }

        [Required]
        public string PrimaryEmail { get; set; }
        public string SecondaryEmail { get; set; } = "";
        //[Required]
        public string Password { get; set; }
        public string OfficePhoneNumber { get; set; } = "";
        [Required]
        public string PersonalMobileNumber { get; set; }
        [Required]
        public int GenderId { get; set; }
        public string Gender { get; set; }
        public string OfficeAddress { get; set; } = "";
        public string HomeAddress { get; set; } = "";
        [Required]
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string City { get; set; } = "";
        public int? StateKey { get; set; } = 0;
        public string State { get; set; } = "";
        public string Zip { get; set; } = "";
        public bool TwoFactorEnabled { get; set; }
        public bool IsActive { get; set; }
        public byte[] UserImageByte { get; set; }
        public string UserImage { get; set; } = "";
        public string RoleIds { get; set; } = "";
        public string orgIds { get; set; } = "";
        public string dptIds { get; set; } = "";
        public string serviceIds { get; set; } = "";
        public List<ServiceLineVM> UserServices { get; set; }
        public List<DepartmentVM> Departments { get; set; }
        public List<OrganizationVM> Organizations { get; set; }

        public List<int> OrgIdsList { get; set; }
        public List<int> DptIdsList { get; set; }
        public List<int> ServiceLineIdsList { get; set; }

        public List<keysVM> selectedNodes { get; set; }
        public List<keysVM> selectedRoles { get; set; }

        public List<UserRoleVM> UserRole { get; set; }
    }

    public class keysVM
    {
        public string Key { get; set; }
    }
}
