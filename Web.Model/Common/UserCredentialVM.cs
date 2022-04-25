﻿namespace Web.Model.Common
{
    public class UserCredentialVM
    {
        public string username { get; set; }
        public string password { get; set; }
        public bool UserIsActive { get; set; }
        public bool OrgIsActive { get; set; }
    }

    public class ForgetPasswordVM
    {
        public string Email { get; set; }
        public string Url { get; set; }
    }

    public class ChangePasswordVM
    {
        public int OrganizationId { get; set; }
        public int UserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public bool isFromProfile { get; set; }
    }
}
