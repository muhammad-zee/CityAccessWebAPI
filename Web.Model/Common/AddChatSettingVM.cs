using System;

namespace Web.Model.Common
{
    public class AddChatSettingVM
    {
        public int ChatSettingId { get; set; }
        public int UserIdFk { get; set; }
        public bool IsMute { get; set; }
        public string CallSound { get; set; }
        public string MessageSound { get; set; }
        public int? FontSize { get; set; }
        public string Wallpaper { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public WallpaperFileVM WallpaperObj { get; set; }

    }

    public class WallpaperFileVM
    {
        public string FileName { get; set; }
        public string Base64Str { get; set; }

    }
}