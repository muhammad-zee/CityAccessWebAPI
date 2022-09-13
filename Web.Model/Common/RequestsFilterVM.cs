using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    class RequestsFilterVM
    {
        public int Dwn { get; set; }
        public string Partner { get; set; }
        public string Operator { get; set; }
        public string Agent { get; set; }
        public string ServTitle { get; set; }


        public string Status { get; set; }
        //public IEnumerable<CityAccess.Req_User> Req_UserList { get; set; }
        //public IEnumerable<CityAccess.Req_Partner> Req_PartnerList { get; set; }
        //public Request request { get; set; }
        //public List<SelectListItem> StatusFilters { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:yyyy-MM-dd}")]
        public Nullable<System.DateTime> Date { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:yyyy-MM-dd}")]
        public Nullable<System.DateTime> Date2 { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:yyyy-MM-dd}")]
        public Nullable<System.DateTime> BookingDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = @"{0:yyyy-MM-dd}")]
        public Nullable<System.DateTime> BookingDate2 { get; set; }

    }
}
