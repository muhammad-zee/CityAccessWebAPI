using System;
using System.Collections.Generic;

#nullable disable

namespace Web.API.Models
{
    public partial class ChatSetting
    {
        public int ChatSettingId { get; set; }
        public int UserIdFk { get; set; }
        public bool IsMute { get; set; }
        public string CallSound { get; set; }
        public string MessageSound { get; set; }
        public string Wallpaper { get; set; }
        public int? FontSize { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual User UserIdFkNavigation { get; set; }
    }
}
