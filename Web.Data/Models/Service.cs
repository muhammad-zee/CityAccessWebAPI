using System;
using System.Collections.Generic;

#nullable disable

namespace Web.Data.Models
{
    public partial class Service
    {
        public Service()
        {
            Agreements = new HashSet<Agreement>();
            Events = new HashSet<Event>();
            ServiceImages = new HashSet<ServiceImage>();
        }

        public int Id { get; set; }
        public int? TypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? CityId { get; set; }
        public bool IsPublic { get; set; }
        public int? OperatorId { get; set; }
        public string FieldName1 { get; set; }
        public string FieldName2 { get; set; }
        public string FieldName3 { get; set; }
        public string FieldName4 { get; set; }
        public string FieldName5 { get; set; }
        public string FieldName6 { get; set; }
        public string FieldName7 { get; set; }
        public string FieldName8 { get; set; }
        public string FieldName9 { get; set; }
        public string FieldName10 { get; set; }
        public string FieldName11 { get; set; }
        public string FieldName12 { get; set; }
        public bool? Field1IsActive { get; set; }
        public bool? Field2IsActive { get; set; }
        public bool? Field3IsActive { get; set; }
        public bool? Field4IsActive { get; set; }
        public bool? Field5IsActive { get; set; }
        public bool? Field6IsActive { get; set; }
        public bool? Field7IsActive { get; set; }
        public bool? Field8IsActive { get; set; }
        public bool? Field9IsActive { get; set; }
        public bool? Field10IsActive { get; set; }
        public bool? Field11IsActive { get; set; }
        public bool? Field12IsActive { get; set; }
        public bool? Field1IsMandatory { get; set; }
        public bool? Field2IsMandatory { get; set; }
        public bool? Field3IsMandatory { get; set; }
        public bool? Field4IsMandatory { get; set; }
        public bool? Field5IsMandatory { get; set; }
        public bool? Field6IsMandatory { get; set; }
        public bool? Field7IsMandatory { get; set; }
        public bool? Field8IsMandatory { get; set; }
        public bool? Field9IsMandatory { get; set; }
        public bool? Field10IsMandatory { get; set; }
        public bool? Field11IsMandatory { get; set; }
        public bool? Field12IsMandatory { get; set; }
        public int? FieldName1Type { get; set; }
        public int? FieldName2Type { get; set; }
        public int? FieldName3Type { get; set; }
        public int? FieldName4Type { get; set; }
        public int? FieldName5Type { get; set; }
        public int? FieldName6Type { get; set; }
        public int? FieldName7Type { get; set; }
        public int? FieldName8Type { get; set; }
        public int? FieldName9Type { get; set; }
        public int? FieldName10Type { get; set; }
        public int? FieldName11Type { get; set; }
        public int? FieldName12Type { get; set; }
        public int? MaxPersonNum { get; set; }
        public int? MinPersonNum { get; set; }
        public bool? Override1 { get; set; }
        public decimal? PaymentAgent { get; set; }
        public bool? ExecutionControl { get; set; }
        public bool? Voucher { get; set; }
        public string AgentInstructions { get; set; }
        public string ConfirmationText { get; set; }
        public string CancellationPolicy { get; set; }
        public decimal? CommissionValue { get; set; }
        public decimal? Price { get; set; }
        public int? PriceType { get; set; }
        public int? ComissionType { get; set; }
        public int? PaymentAgentType { get; set; }
        public int? Availability1 { get; set; }
        public bool? IsActive { get; set; }
        public TimeSpan? Duration { get; set; }

        public virtual DynamicFieldAlternative Availability1Navigation { get; set; }
        public virtual City City { get; set; }
        public virtual DynamicFieldAlternative ComissionTypeNavigation { get; set; }
        public virtual Partner Operator { get; set; }
        public virtual DynamicFieldAlternative PaymentAgentTypeNavigation { get; set; }
        public virtual DynamicFieldAlternative PriceTypeNavigation { get; set; }
        public virtual ServiceType Type { get; set; }
        public virtual ICollection<Agreement> Agreements { get; set; }
        public virtual ICollection<Event> Events { get; set; }
        public virtual ICollection<ServiceImage> ServiceImages { get; set; }
    }
}
