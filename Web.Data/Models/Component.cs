﻿using System;

#nullable disable

namespace Web.Data.Models
{
    public partial class Component
    {
        public Component()
        {
            ComponentAccesses = new HashSet<ComponentAccess>();
            UserAccesses = new HashSet<UserAccess>();
        }

        public int ComponentId { get; set; }
        public string ComModuleName { get; set; }
        public int? ParentComponentId { get; set; }
        public string PageUrl { get; set; }
        public string PageName { get; set; }
        public string PageTitle { get; set; }
        public string PageDescription { get; set; }
        public bool Status { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public int? SortOrder { get; set; }
        public string ModuleImage { get; set; }

        public virtual ICollection<ComponentAccess> ComponentAccesses { get; set; }
        public virtual ICollection<UserAccess> UserAccesses { get; set; }
    }
}
