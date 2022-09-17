using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class RequestsCalendarViewVM
    {
        public DateTime Date { get; set; }
        public IList<RequestsTimeSlots> Slotes{ get; set; }
    }
    public class RequestsTimeSlots
    {
        public TimeSpan Time { get; set; }
        public IList<RequestVM> Requests { get; set; }
    }
}
