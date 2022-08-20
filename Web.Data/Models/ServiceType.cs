using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class ServiceType
    {
        public ServiceType()
        {
            Services = new HashSet<Service>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool HasReturn { get; set; }
        public bool? IsTransfer { get; set; }
        public string LanguageId { get; set; }

        public virtual Language Language { get; set; }
        public virtual ICollection<Service> Services { get; set; }
    }
}
