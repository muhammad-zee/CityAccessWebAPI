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
        public DateTime? Dob { get; set; }
        public string DobStr { get; set; }
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
        public string DepartmentIds { get; set; } = "";
        public string ServiceLineIds { get; set; } = "";
        public string Zip { get; set; } = "";
        public bool TwoFactorEnabled { get; set; }
        public bool IsActive { get; set; }
        public byte[] UserImageByte { get; set; }
        public string UserImage { get; set; } = "";
        public string RoleIds { get; set; } = "";
        public string orgIds { get; set; } = "";
        public string dptIds { get; set; } = "";
        public List<int> serviceIdsFT { get; set; }
        public List<int> dptIdsFT { get; set; }
        public List<int?> orgIdsFT { get; set; }
        public string serviceIds { get; set; } = "";

        public bool IsAfterHours { get; set; }
        public string UserUniqueId { get; set; }

        public bool status { get; set; }

        public List<ServiceLineVM> UserServices { get; set; }
        public List<DepartmentVM> Departments { get; set; }
        public List<OrganizationVM> Organizations { get; set; }

        public List<int> OrgIdsList { get; set; }
        public List<int> DptIdsList { get; set; }
        public List<int> ServiceLineIdsList { get; set; }

        public List<UserProfileSchedulesVM> UserProfileSchedules { get; set; }
        public List<UserRoleVM> UserRole { get; set; }

        ///// For Get All Users Pagination
        public int OrganizationId { get; set; }
        public int Total_Records { get; set; }
        public int PageNumber { get; set; }
        public int Rows { get; set; }
        public string Filter { get; set; }
        public string SortOrder { get; set; }
        public string SortCol { get; set; }
        public string FilterVal { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool IsEMS { get; set; } = false;
        public bool IsDiscoveredByOtherOrganization { get; set; }
    }

    public class UserProfileSchedulesVM
    {
        public string RoleName { get; set; }
        public string ServiceLine { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string DayOfWeek { get; set; }
    }
}
