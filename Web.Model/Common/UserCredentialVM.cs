﻿namespace Web.Model.Common
{
    public class UserCredentialVM
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class ForgetPasswordVM
    {
        public string Email { get; set; }
        public string Url { get; set; }
    }
}
