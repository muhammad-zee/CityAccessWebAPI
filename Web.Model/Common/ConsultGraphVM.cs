using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web.Model.Common
{
    public class ConsultGraphVM
    {
        public DateTime CreatedDate { get; set; }
        public int UrgentConsults { get; set; }
        public int RoutineConsults { get; set; }

    }
}
