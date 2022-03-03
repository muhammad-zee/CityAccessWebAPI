using System;

namespace Web.Model.Common
{
    public class GraphVM
    {
        public DateTime CreatedDate { get; set; }
        public int UrgentConsults { get; set; }
        public int RoutineConsults { get; set; }
        public int EMS { get; set; }
        public int ActiveCodes { get; set; }
    }
}
