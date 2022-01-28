﻿using System.Collections.Generic;

namespace Web.Model.Common
{
    public class ChatUsersVM
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string PrimaryEmail { get; set; }
        public string PersonalMobileNumber { get; set; }
        public string UserImage { get; set; }
        public string UserChannelSid { get; set; }
        public string UserUniqueId { get; set; }

        public List<UserRoleVM> UserRoles { get; set; }
        public List<ServiceLineVM> ServiceLines { get; set; }
    }
}
