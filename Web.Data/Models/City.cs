using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class City
    {
        public City()
        {
            Services = new HashSet<Service>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Service> Services { get; set; }
    }
}
