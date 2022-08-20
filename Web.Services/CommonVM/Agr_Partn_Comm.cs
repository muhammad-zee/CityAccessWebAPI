using Web.Data.Models;

namespace Web.Services.CommonVM
{
    internal class Agr_Partn_Comm
    {
        public Agreement Agreement { get; set; }
        public Partner Partner { get; set; }
        public string BaseService { get; set; }
        public ServiceImage serviceImage { get; set; }
    }
}