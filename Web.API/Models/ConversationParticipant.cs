﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class ConversationParticipant
    {
        public int ConversationParticipantId { get; set; }
        public string FriendlyName { get; set; }
        public string UniqueName { get; set; }
        public bool IsAdmin { get; set; }
        public int UserIdFk { get; set; }
        public int ConversationChannelIdFk { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ConversationChannel ConversationChannelIdFkNavigation { get; set; }
        public virtual User UserIdFkNavigation { get; set; }
    }
}