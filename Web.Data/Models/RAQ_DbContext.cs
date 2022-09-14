using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Web.Data.Models
{
    public partial class RAQ_DbContext : DbContext
    {
        public RAQ_DbContext()
        {
        }

        public RAQ_DbContext(DbContextOptions<RAQ_DbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Agreement> Agreements { get; set; }
        public virtual DbSet<AgreementLog> AgreementLogs { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<CommissionType> CommissionTypes { get; set; }
        public virtual DbSet<DynamicField> DynamicFields { get; set; }
        public virtual DbSet<DynamicFieldAlternative> DynamicFieldAlternatives { get; set; }
        public virtual DbSet<DynamicFieldAlternativeMl> DynamicFieldAlternativeMls { get; set; }
        public virtual DbSet<DynamicFieldMl> DynamicFieldMls { get; set; }
        public virtual DbSet<ElmahError> ElmahErrors { get; set; }
        public virtual DbSet<ErrorLog> ErrorLogs { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<EventState> EventStates { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<Partner> Partners { get; set; }
        public virtual DbSet<PartnerLogo> PartnerLogos { get; set; }
        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<RequestLog> RequestLogs { get; set; }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<ServiceImage> ServiceImages { get; set; }
        public virtual DbSet<ServiceType> ServiceTypes { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<StateTransition> StateTransitions { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserLog> UserLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=cityAccess.mssql.somee.com;Initial Catalog=cityAccess;User Id=Suffiullah0002_SQLLogin_1;Password=xk36yc6rim;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Agreement>(entity =>
            {
                entity.ToTable("Agreement");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AgentInstructions)
                    .HasMaxLength(1500)
                    .HasColumnName("agentInstructions");

                entity.Property(e => e.CancellationPolicy)
                    .HasMaxLength(1500)
                    .HasColumnName("cancellationPolicy");

                entity.Property(e => e.CommissionType).HasColumnName("commissionType");

                entity.Property(e => e.CommissionValue)
                    .HasColumnType("decimal(15, 2)")
                    .HasColumnName("commissionValue");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");

                entity.Property(e => e.EmailToCustomer)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("isActive")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsConfirmed).HasColumnName("isConfirmed");

                entity.Property(e => e.Label)
                    .HasMaxLength(50)
                    .HasColumnName("label");

                entity.Property(e => e.MessageTemplate)
                    .HasMaxLength(1500)
                    .HasColumnName("messageTemplate");

                entity.Property(e => e.NeedsApproval)
                    .IsRequired()
                    .HasColumnName("needsApproval")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.PartnerId).HasColumnName("partnerID");

                entity.Property(e => e.PaymentAgent).HasColumnType("decimal(15, 2)");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(15, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.PriceType).HasColumnName("priceType");

                entity.Property(e => e.ServiceId).HasColumnName("serviceID");

                entity.HasOne(d => d.CommissionTypeNavigation)
                    .WithMany(p => p.Agreements)
                    .HasForeignKey(d => d.CommissionType)
                    .HasConstraintName("FK_Agreement_commissionType");

                entity.HasOne(d => d.Partner)
                    .WithMany(p => p.Agreements)
                    .HasForeignKey(d => d.PartnerId)
                    .HasConstraintName("FK_Agreement_Partner");

                entity.HasOne(d => d.PaymentAgentTypeNavigation)
                    .WithMany(p => p.AgreementPaymentAgentTypeNavigations)
                    .HasForeignKey(d => d.PaymentAgentType)
                    .HasConstraintName("FK_Agreement_DynamicFieldAlternative3");

                entity.HasOne(d => d.PriceTypeNavigation)
                    .WithMany(p => p.AgreementPriceTypeNavigations)
                    .HasForeignKey(d => d.PriceType)
                    .HasConstraintName("FK_Agreement_DynamicFieldAlternative");

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.Agreements)
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Agreement_Service");

                entity.HasOne(d => d.TypeCommissionNavigation)
                    .WithMany(p => p.AgreementTypeCommissionNavigations)
                    .HasForeignKey(d => d.TypeCommission)
                    .HasConstraintName("FK_Agreement_DynamicFieldAlternative2");
            });

            modelBuilder.Entity<AgreementLog>(entity =>
            {
                entity.ToTable("AgreementLog");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AgreementId).HasColumnName("agreementID");

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.Notes)
                    .HasMaxLength(1000)
                    .HasColumnName("notes");

                entity.Property(e => e.Time)
                    .IsRequired()
                    .HasMaxLength(7);

                entity.Property(e => e.UserId).HasColumnName("userID");

                entity.HasOne(d => d.Agreement)
                    .WithMany(p => p.AgreementLogs)
                    .HasForeignKey(d => d.AgreementId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AgreementLog_Request");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AgreementLogs)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AgreementLog_User");
            });

            modelBuilder.Entity<City>(entity =>
            {
                entity.ToTable("City");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<CommissionType>(entity =>
            {
                entity.ToTable("commissionType");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnName("description");

                entity.Property(e => e.Label)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("label");
            });

            modelBuilder.Entity<DynamicField>(entity =>
            {
                entity.ToTable("DynamicField");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.FieldType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("fieldType");
            });

            modelBuilder.Entity<DynamicFieldAlternative>(entity =>
            {
                entity.ToTable("DynamicFieldAlternative");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.DynamicfieldId).HasColumnName("dynamicfieldID");

                entity.Property(e => e.Label)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("label");

                entity.HasOne(d => d.Dynamicfield)
                    .WithMany(p => p.DynamicFieldAlternatives)
                    .HasForeignKey(d => d.DynamicfieldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DynamicFieldAlternative_DynamicField");
            });

            modelBuilder.Entity<DynamicFieldAlternativeMl>(entity =>
            {
                entity.HasKey(e => new { e.LanguageId, e.DynamicfieldalternativeId });

                entity.ToTable("DynamicFieldAlternativeMl");

                entity.Property(e => e.LanguageId)
                    .HasMaxLength(5)
                    .HasColumnName("languageID");

                entity.Property(e => e.DynamicfieldalternativeId).HasColumnName("dynamicfieldalternativeID");

                entity.Property(e => e.Label)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("label");

                entity.HasOne(d => d.Dynamicfieldalternative)
                    .WithMany(p => p.DynamicFieldAlternativeMls)
                    .HasForeignKey(d => d.DynamicfieldalternativeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DynamicFieldAlternativeMl_DynamicFieldAlternative");

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.DynamicFieldAlternativeMls)
                    .HasForeignKey(d => d.LanguageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DynamicFieldAlternativeMl_Language");
            });

            modelBuilder.Entity<DynamicFieldMl>(entity =>
            {
                entity.HasKey(e => new { e.LanguageId, e.DynamicfieldId });

                entity.ToTable("DynamicFieldMl");

                entity.Property(e => e.LanguageId)
                    .HasMaxLength(5)
                    .HasColumnName("languageID");

                entity.Property(e => e.DynamicfieldId).HasColumnName("dynamicfieldID");

                entity.Property(e => e.Label)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("label");

                entity.HasOne(d => d.Dynamicfield)
                    .WithMany(p => p.DynamicFieldMls)
                    .HasForeignKey(d => d.DynamicfieldId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DynamicFieldMl_DynamicField");

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.DynamicFieldMls)
                    .HasForeignKey(d => d.LanguageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DynamicFieldMl_Language");
            });

            modelBuilder.Entity<ElmahError>(entity =>
            {
                entity.HasKey(e => e.ErrorId)
                    .IsClustered(false);

                entity.ToTable("ELMAH_Error");

                entity.HasIndex(e => new { e.Application, e.TimeUtc, e.Sequence }, "IX_ELMAH_Error_App_Time_Seq");

                entity.Property(e => e.ErrorId).HasDefaultValueSql("(newid())");

                entity.Property(e => e.AllXml).IsRequired();

                entity.Property(e => e.Application)
                    .IsRequired()
                    .HasMaxLength(60);

                entity.Property(e => e.Host)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Message)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Sequence).ValueGeneratedOnAdd();

                entity.Property(e => e.Source)
                    .IsRequired()
                    .HasMaxLength(60);

                entity.Property(e => e.TimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.User)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<ErrorLog>(entity =>
            {
                entity.ToTable("ErrorLog");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ErrorDate)
                    .HasColumnType("date")
                    .HasColumnName("errorDate");

                entity.Property(e => e.ErrorMsg)
                    .IsRequired()
                    .HasColumnName("errorMsg");

                entity.Property(e => e.ErrorTime).HasColumnName("errorTime");

                entity.Property(e => e.ErrorUrl)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("errorURL");

                entity.HasOne(d => d.UserNavigation)
                    .WithMany(p => p.ErrorLogs)
                    .HasForeignKey(d => d.User)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ErrorLog_User");
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("Event");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.EndTime).HasColumnName("endTime");

                entity.Property(e => e.EventDate)
                    .HasColumnType("date")
                    .HasColumnName("eventDate");

                entity.Property(e => e.MaxPersons).HasColumnName("maxPersons");

                entity.Property(e => e.Notes)
                    .HasMaxLength(1000)
                    .HasColumnName("notes");

                entity.Property(e => e.ServiceId).HasColumnName("serviceID");

                entity.Property(e => e.StartTime).HasColumnName("startTime");

                entity.Property(e => e.StateId)
                    .HasMaxLength(20)
                    .HasColumnName("stateID");

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.Events)
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Event_Service");

                entity.HasOne(d => d.State)
                    .WithMany(p => p.Events)
                    .HasForeignKey(d => d.StateId)
                    .HasConstraintName("FK_Event_EventState");
            });

            modelBuilder.Entity<EventState>(entity =>
            {
                entity.ToTable("EventState");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("ID");
            });

            modelBuilder.Entity<Language>(entity =>
            {
                entity.ToTable("Language");

                entity.Property(e => e.Id)
                    .HasMaxLength(5)
                    .HasColumnName("ID");

                entity.Property(e => e.LanguageLabel)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasColumnName("languageLabel");
            });

            modelBuilder.Entity<Partner>(entity =>
            {
                entity.ToTable("Partner");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ContactEmail)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContactPerson)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.ContactPhone)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CountryId)
                    .HasMaxLength(50)
                    .HasColumnName("countryID");

                entity.Property(e => e.Description)
                    .HasMaxLength(550)
                    .HasColumnName("description");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .HasColumnName("email");

                entity.Property(e => e.FiscalId)
                    .HasMaxLength(50)
                    .HasColumnName("fiscalID");

                entity.Property(e => e.Invitedby).HasColumnName("invitedby");

                entity.Property(e => e.InvoiceAddress)
                    .HasMaxLength(500)
                    .HasColumnName("invoiceAddress");

                entity.Property(e => e.InvoiceName)
                    .HasMaxLength(50)
                    .HasColumnName("invoiceName");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasColumnName("isActive")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsAgent).HasColumnName("isAgent");

                entity.Property(e => e.IsOperator).HasColumnName("isOperator");

                entity.Property(e => e.IsPublic).HasColumnName("isPublic");

                entity.Property(e => e.IsTest).HasColumnName("isTest");

                entity.Property(e => e.TradeName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("tradeName");
            });

            modelBuilder.Entity<PartnerLogo>(entity =>
            {
                entity.ToTable("PartnerLogo");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.PartnerId).HasColumnName("PartnerID");

                entity.HasOne(d => d.Partner)
                    .WithMany(p => p.PartnerLogos)
                    .HasForeignKey(d => d.PartnerId)
                    .HasConstraintName("FK_Partner_ID");
            });

            modelBuilder.Entity<Request>(entity =>
            {
                entity.ToTable("Request");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AgreementId).HasColumnName("agreementID");

                entity.Property(e => e.BookDate)
                    .HasColumnType("date")
                    .HasColumnName("bookDate");

                entity.Property(e => e.BookTime).HasColumnName("bookTime");

                entity.Property(e => e.BookerId).HasColumnName("bookerId");

                entity.Property(e => e.ClientNotes)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ContactEmail)
                    .HasMaxLength(50)
                    .HasColumnName("contactEmail");

                entity.Property(e => e.ContactName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("contactName");

                entity.Property(e => e.ContactPhone)
                    .HasMaxLength(20)
                    .HasColumnName("contactPhone")
                    .IsFixedLength(true);

                entity.Property(e => e.DropoffLocation)
                    .HasMaxLength(100)
                    .HasColumnName("dropoffLocation");

                entity.Property(e => e.EventDate)
                    .HasColumnType("date")
                    .HasColumnName("eventDate");

                entity.Property(e => e.EventId).HasColumnName("eventID");

                entity.Property(e => e.EventTime).HasColumnName("eventTime");

                entity.Property(e => e.ExtraDate1)
                    .HasColumnType("date")
                    .HasColumnName("extraDate1");

                entity.Property(e => e.ExtraDate2)
                    .HasColumnType("date")
                    .HasColumnName("extraDate2");

                entity.Property(e => e.ExtraDate3)
                    .HasColumnType("date")
                    .HasColumnName("extraDate3");

                entity.Property(e => e.ExtraMultiText1)
                    .HasMaxLength(1000)
                    .HasColumnName("extraMultiText1");

                entity.Property(e => e.ExtraMultiText2)
                    .HasMaxLength(1000)
                    .HasColumnName("extraMultiText2");

                entity.Property(e => e.ExtraMultiText3)
                    .HasMaxLength(1000)
                    .HasColumnName("extraMultiText3");

                entity.Property(e => e.ExtraText1)
                    .HasMaxLength(100)
                    .HasColumnName("extraText1");

                entity.Property(e => e.ExtraText2)
                    .HasMaxLength(100)
                    .HasColumnName("extraText2");

                entity.Property(e => e.ExtraText3)
                    .HasMaxLength(100)
                    .HasColumnName("extraText3");

                entity.Property(e => e.ExtraTime1).HasColumnName("extraTime1");

                entity.Property(e => e.ExtraTime2).HasColumnName("extraTime2");

                entity.Property(e => e.ExtraTime3).HasColumnName("extraTime3");

                entity.Property(e => e.FlightNr)
                    .HasMaxLength(20)
                    .HasColumnName("flightNr")
                    .IsFixedLength(true);

                entity.Property(e => e.Notes)
                    .HasMaxLength(1000)
                    .HasColumnName("notes");

                entity.Property(e => e.NrPersons).HasColumnName("nrPersons");

                entity.Property(e => e.OperatorNotes).HasMaxLength(1000);

                entity.Property(e => e.PickupLocation)
                    .HasMaxLength(100)
                    .HasColumnName("pickupLocation");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(16, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.Reference)
                    .HasMaxLength(50)
                    .HasColumnName("reference");

                entity.Property(e => e.ReturnDate)
                    .HasColumnType("date")
                    .HasColumnName("returnDate");

                entity.Property(e => e.ReturnDropoff)
                    .HasMaxLength(100)
                    .HasColumnName("returnDropoff");

                entity.Property(e => e.ReturnFlight)
                    .HasMaxLength(20)
                    .HasColumnName("returnFlight")
                    .IsFixedLength(true);

                entity.Property(e => e.ReturnPickup)
                    .HasMaxLength(100)
                    .HasColumnName("returnPickup");

                entity.Property(e => e.ReturnTime).HasColumnName("returnTime");

                entity.Property(e => e.StateId)
                    .HasMaxLength(20)
                    .HasColumnName("stateID");

                entity.HasOne(d => d.Agreement)
                    .WithMany(p => p.Requests)
                    .HasForeignKey(d => d.AgreementId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Request_Agreement");

                entity.HasOne(d => d.Booker)
                    .WithMany(p => p.RequestBookers)
                    .HasForeignKey(d => d.BookerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Request_User");

                entity.HasOne(d => d.Event)
                    .WithMany(p => p.Requests)
                    .HasForeignKey(d => d.EventId)
                    .HasConstraintName("FK_Request_Event");

                entity.HasOne(d => d.Responsible)
                    .WithMany(p => p.RequestResponsibles)
                    .HasForeignKey(d => d.ResponsibleId)
                    .HasConstraintName("FK_Request_User2");

                entity.HasOne(d => d.State)
                    .WithMany(p => p.Requests)
                    .HasForeignKey(d => d.StateId)
                    .HasConstraintName("FK_Request_State");
            });

            modelBuilder.Entity<RequestLog>(entity =>
            {
                entity.ToTable("RequestLog");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.Notes)
                    .HasMaxLength(1000)
                    .HasColumnName("notes");

                entity.Property(e => e.RequestId).HasColumnName("requestID");

                entity.Property(e => e.Time)
                    .IsRequired()
                    .HasMaxLength(7);

                entity.Property(e => e.UserId).HasColumnName("userID");

                entity.HasOne(d => d.Request)
                    .WithMany(p => p.RequestLogs)
                    .HasForeignKey(d => d.RequestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RequestLog_Request");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.RequestLogs)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RequestLog_User");
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("Service");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AgentInstructions)
                    .HasMaxLength(1500)
                    .HasColumnName("agentInstructions");

                entity.Property(e => e.CancellationPolicy)
                    .HasMaxLength(1500)
                    .HasColumnName("cancellationPolicy");

                entity.Property(e => e.CityId).HasColumnName("cityID");

                entity.Property(e => e.ComissionType).HasColumnName("comissionType");

                entity.Property(e => e.CommissionValue)
                    .HasColumnType("decimal(15, 2)")
                    .HasColumnName("commissionValue");

                entity.Property(e => e.ConfirmationText).HasMaxLength(1500);

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasColumnName("description");

                entity.Property(e => e.Field10IsActive).HasColumnName("field10IsActive");

                entity.Property(e => e.Field10IsMandatory).HasColumnName("field10IsMandatory");

                entity.Property(e => e.Field11IsActive).HasColumnName("field11IsActive");

                entity.Property(e => e.Field11IsMandatory).HasColumnName("field11IsMandatory");

                entity.Property(e => e.Field12IsActive).HasColumnName("field12IsActive");

                entity.Property(e => e.Field12IsMandatory).HasColumnName("field12IsMandatory");

                entity.Property(e => e.Field1IsActive).HasColumnName("field1IsActive");

                entity.Property(e => e.Field1IsMandatory).HasColumnName("field1IsMandatory");

                entity.Property(e => e.Field2IsActive).HasColumnName("field2IsActive");

                entity.Property(e => e.Field2IsMandatory).HasColumnName("field2IsMandatory");

                entity.Property(e => e.Field3IsActive).HasColumnName("field3IsActive");

                entity.Property(e => e.Field3IsMandatory).HasColumnName("field3IsMandatory");

                entity.Property(e => e.Field4IsActive).HasColumnName("field4IsActive");

                entity.Property(e => e.Field4IsMandatory).HasColumnName("field4IsMandatory");

                entity.Property(e => e.Field5IsActive).HasColumnName("field5IsActive");

                entity.Property(e => e.Field5IsMandatory).HasColumnName("field5IsMandatory");

                entity.Property(e => e.Field6IsActive).HasColumnName("field6IsActive");

                entity.Property(e => e.Field6IsMandatory).HasColumnName("field6IsMandatory");

                entity.Property(e => e.Field7IsActive).HasColumnName("field7IsActive");

                entity.Property(e => e.Field7IsMandatory).HasColumnName("field7IsMandatory");

                entity.Property(e => e.Field8IsActive).HasColumnName("field8IsActive");

                entity.Property(e => e.Field8IsMandatory).HasColumnName("field8IsMandatory");

                entity.Property(e => e.Field9IsActive).HasColumnName("field9IsActive");

                entity.Property(e => e.Field9IsMandatory).HasColumnName("field9IsMandatory");

                entity.Property(e => e.FieldName1)
                    .HasMaxLength(50)
                    .HasColumnName("fieldName1");

                entity.Property(e => e.FieldName10)
                    .HasMaxLength(50)
                    .HasColumnName("fieldName10");

                entity.Property(e => e.FieldName10Type).HasColumnName("fieldName10Type");

                entity.Property(e => e.FieldName11)
                    .HasMaxLength(50)
                    .HasColumnName("fieldName11");

                entity.Property(e => e.FieldName11Type).HasColumnName("fieldName11Type");

                entity.Property(e => e.FieldName12)
                    .HasMaxLength(50)
                    .HasColumnName("fieldName12");

                entity.Property(e => e.FieldName12Type).HasColumnName("fieldName12Type");

                entity.Property(e => e.FieldName1Type).HasColumnName("fieldName1Type");

                entity.Property(e => e.FieldName2)
                    .HasMaxLength(50)
                    .HasColumnName("fieldName2");

                entity.Property(e => e.FieldName2Type).HasColumnName("fieldName2Type");

                entity.Property(e => e.FieldName3)
                    .HasMaxLength(50)
                    .HasColumnName("fieldName3");

                entity.Property(e => e.FieldName3Type).HasColumnName("fieldName3Type");

                entity.Property(e => e.FieldName4)
                    .HasMaxLength(50)
                    .HasColumnName("fieldName4");

                entity.Property(e => e.FieldName4Type).HasColumnName("fieldName4Type");

                entity.Property(e => e.FieldName5)
                    .HasMaxLength(50)
                    .HasColumnName("fieldName5");

                entity.Property(e => e.FieldName5Type).HasColumnName("fieldName5Type");

                entity.Property(e => e.FieldName6)
                    .HasMaxLength(50)
                    .HasColumnName("fieldName6");

                entity.Property(e => e.FieldName6Type).HasColumnName("fieldName6Type");

                entity.Property(e => e.FieldName7)
                    .HasMaxLength(50)
                    .HasColumnName("fieldName7");

                entity.Property(e => e.FieldName7Type).HasColumnName("fieldName7Type");

                entity.Property(e => e.FieldName8)
                    .HasMaxLength(50)
                    .HasColumnName("fieldName8");

                entity.Property(e => e.FieldName8Type).HasColumnName("fieldName8Type");

                entity.Property(e => e.FieldName9)
                    .HasMaxLength(50)
                    .HasColumnName("fieldName9");

                entity.Property(e => e.FieldName9Type).HasColumnName("fieldName9Type");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.IsPublic).HasColumnName("isPublic");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.OperatorId).HasColumnName("operatorID");

                entity.Property(e => e.PaymentAgent).HasColumnType("decimal(15, 2)");

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(15, 2)")
                    .HasColumnName("price");

                entity.Property(e => e.PriceType).HasColumnName("priceType");

                entity.Property(e => e.TypeId).HasColumnName("typeID");

                entity.HasOne(d => d.Availability1Navigation)
                    .WithMany(p => p.ServiceAvailability1Navigations)
                    .HasForeignKey(d => d.Availability1)
                    .HasConstraintName("FK_Service_DynamicFieldAlternative4");

                entity.HasOne(d => d.City)
                    .WithMany(p => p.Services)
                    .HasForeignKey(d => d.CityId)
                    .HasConstraintName("FK_Service_City");

                entity.HasOne(d => d.ComissionTypeNavigation)
                    .WithMany(p => p.ServiceComissionTypeNavigations)
                    .HasForeignKey(d => d.ComissionType)
                    .HasConstraintName("FK_Service_DynamicFieldAlternative2");

                entity.HasOne(d => d.Operator)
                    .WithMany(p => p.Services)
                    .HasForeignKey(d => d.OperatorId)
                    .HasConstraintName("FK_Service_Partner");

                entity.HasOne(d => d.PaymentAgentTypeNavigation)
                    .WithMany(p => p.ServicePaymentAgentTypeNavigations)
                    .HasForeignKey(d => d.PaymentAgentType)
                    .HasConstraintName("FK_Service_DynamicFieldAlternative3");

                entity.HasOne(d => d.PriceTypeNavigation)
                    .WithMany(p => p.ServicePriceTypeNavigations)
                    .HasForeignKey(d => d.PriceType)
                    .HasConstraintName("FK_Service_DynamicFieldAlternative");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Services)
                    .HasForeignKey(d => d.TypeId)
                    .HasConstraintName("FK_Service_serviceType");
            });

            modelBuilder.Entity<ServiceImage>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.SequenceNr).HasColumnName("sequenceNR");

                entity.Property(e => e.ServiceId).HasColumnName("serviceID");

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.ServiceImages)
                    .HasForeignKey(d => d.ServiceId)
                    .HasConstraintName("FK_Service_ID");
            });

            modelBuilder.Entity<ServiceType>(entity =>
            {
                entity.ToTable("serviceType");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("description");

                entity.Property(e => e.HasReturn).HasColumnName("hasReturn");

                entity.Property(e => e.IsTransfer)
                    .HasColumnName("isTransfer")
                    .HasDefaultValueSql("((0))");

                entity.Property(e => e.LanguageId)
                    .HasMaxLength(5)
                    .HasColumnName("LanguageID");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.ServiceTypes)
                    .HasForeignKey(d => d.LanguageId)
                    .HasConstraintName("FK_serviceType_Language");
            });

            modelBuilder.Entity<State>(entity =>
            {
                entity.ToTable("State");

                entity.Property(e => e.Id)
                    .HasMaxLength(20)
                    .HasColumnName("ID");
            });

            modelBuilder.Entity<StateTransition>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Destiny).HasMaxLength(30);

                entity.Property(e => e.Origin).HasMaxLength(30);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .HasColumnName("email");

                entity.Property(e => e.EmailConfirmed).HasColumnName("emailConfirmed");

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("fullName");

                entity.Property(e => e.IsActive)
                    .HasColumnName("isActive")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.IsAdmin).HasColumnName("isAdmin");

                entity.Property(e => e.LastLoginDate)
                    .HasColumnType("datetime")
                    .HasColumnName("lastLoginDate");

                entity.Property(e => e.PartnerId).HasColumnName("partnerId");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("password");

                entity.Property(e => e.Phone)
                    .HasMaxLength(50)
                    .HasColumnName("phone");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("username");

                entity.HasOne(d => d.Partner)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.PartnerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Partner");
            });

            modelBuilder.Entity<UserLog>(entity =>
            {
                entity.ToTable("UserLog");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.EditorId).HasColumnName("editorID");

                entity.Property(e => e.Notes)
                    .HasMaxLength(1000)
                    .HasColumnName("notes");

                entity.Property(e => e.Time)
                    .IsRequired()
                    .HasMaxLength(7);

                entity.Property(e => e.UserId).HasColumnName("userID");

                entity.HasOne(d => d.Editor)
                    .WithMany(p => p.UserLogEditors)
                    .HasForeignKey(d => d.EditorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserLog_Editor");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserLogUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserLog_User");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
