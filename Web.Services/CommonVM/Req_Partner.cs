using System.Collections.Generic;
using Web.Data.Models;
using Web.Services.CommonVM;

namespace Web.Services.CommonVM
{
    public class Req_Partner
    {
        internal ServiceType ServiceType;

        public Req_forTransfer req_ForTransfer { get; set; }
        public Request Request { get; set; }
        public Partner Partner { get; set; }
        public Agreement Agreement { get; set; }
        //public serviceType ServiceType { get; set; }
        //public IEnumerable<CityAccess.State> States { get; set; }
    }
}