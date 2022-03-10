using Microsoft.EntityFrameworkCore;

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

        public virtual DbSet<ActiveCode> ActiveCodes { get; set; }
        public virtual DbSet<ActiveCodesGroupMember> ActiveCodesGroupMembers { get; set; }
        public virtual DbSet<ClinicalHoliday> ClinicalHolidays { get; set; }
        public virtual DbSet<ClinicalHour> ClinicalHours { get; set; }
        public virtual DbSet<CodeBlue> CodeBlues { get; set; }
        public virtual DbSet<CodeSepsi> CodeSepses { get; set; }
        public virtual DbSet<CodeStemi> CodeStemis { get; set; }
        public virtual DbSet<CodeStroke> CodeStrokes { get; set; }
        public virtual DbSet<CodeTrauma> CodeTraumas { get; set; }
        public virtual DbSet<CodesServiceLinesMapping> CodesServiceLinesMappings { get; set; }
        public virtual DbSet<Component> Components { get; set; }
        public virtual DbSet<ComponentAccess> ComponentAccesses { get; set; }
        public virtual DbSet<Consult> Consults { get; set; }
        public virtual DbSet<ConsultAcknowledgment> ConsultAcknowledgments { get; set; }
        public virtual DbSet<ConsultField> ConsultFields { get; set; }
        public virtual DbSet<ControlList> ControlLists { get; set; }
        public virtual DbSet<ControlListDetail> ControlListDetails { get; set; }
        public virtual DbSet<ConversationChannel> ConversationChannels { get; set; }
        public virtual DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<ElmahError> ElmahErrors { get; set; }
        public virtual DbSet<FavouriteTeam> FavouriteTeams { get; set; }
        public virtual DbSet<InteractiveVoiceResponse> InteractiveVoiceResponses { get; set; }
        public virtual DbSet<Ivrsetting> Ivrsettings { get; set; }
        public virtual DbSet<MdrouteCounter> MdrouteCounters { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public virtual DbSet<OrganizationConsultField> OrganizationConsultFields { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<ServiceLine> ServiceLines { get; set; }
        public virtual DbSet<Setting> Settings { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<Temp> Temps { get; set; }
        public virtual DbSet<TmpFerdeen> TmpFerdeens { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserAccess> UserAccesses { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<UsersRelation> UsersRelations { get; set; }
        public virtual DbSet<UsersSchedule> UsersSchedules { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=192.168.0.4;Initial Catalog=RouteAndQueue;User Id=sa;Password=4292;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<ActiveCode>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.ServiceLineIds).IsRequired();

                entity.HasOne(d => d.OrganizationIdFkNavigation)
                    .WithMany(p => p.ActiveCodesNavigation)
                    .HasForeignKey(d => d.OrganizationIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ActiveCodes_Organizations");
            });

            modelBuilder.Entity<ActiveCodesGroupMember>(entity =>
            {
                entity.Property(e => e.ActiveCodeName)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<ClinicalHoliday>(entity =>
            {
                entity.ToTable("ClinicalHoliday");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.HasOne(d => d.ServicelineIdFkNavigation)
                    .WithMany(p => p.ClinicalHolidays)
                    .HasForeignKey(d => d.ServicelineIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ServiceLine_clinicalHoliday");
            });

            modelBuilder.Entity<ClinicalHour>(entity =>
            {
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EndBreak).HasColumnType("datetime");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.StartBreak).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.StartTime).HasColumnType("datetime");

                entity.HasOne(d => d.ServicelineIdFkNavigation)
                    .WithMany(p => p.ClinicalHours)
                    .HasForeignKey(d => d.ServicelineIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ServiceLine_Departments");
            });

            modelBuilder.Entity<CodeBlue>(entity =>
            {
                entity.ToTable("CodeBlue");

                entity.Property(e => e.Attachments).HasMaxLength(500);

                entity.Property(e => e.Audio).HasMaxLength(500);

                entity.Property(e => e.BloodThinners).HasMaxLength(100);

                entity.Property(e => e.ChiefComplant).HasMaxLength(200);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Distance).HasMaxLength(20);

                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("DOB");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.EstimatedTime).HasMaxLength(50);

                entity.Property(e => e.FamilyContactName).HasMaxLength(100);

                entity.Property(e => e.FamilyContactNumber).HasMaxLength(50);

                entity.Property(e => e.Hpi)
                    .HasMaxLength(500)
                    .HasColumnName("HPI");

                entity.Property(e => e.IsEms).HasColumnName("IsEMS");

                entity.Property(e => e.LastKnownWell).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.PatientName).HasMaxLength(100);

                entity.Property(e => e.StartingPoint).HasMaxLength(50);

                entity.Property(e => e.Video).HasMaxLength(500);
            });

            modelBuilder.Entity<CodeSepsi>(entity =>
            {
                entity.HasKey(e => e.CodeSepsisId);

                entity.ToTable("CodeSepsis");

                entity.Property(e => e.Attachments).HasMaxLength(500);

                entity.Property(e => e.Audio).HasMaxLength(500);

                entity.Property(e => e.BloodThinners).HasMaxLength(100);

                entity.Property(e => e.ChiefComplant).HasMaxLength(200);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Distance).HasMaxLength(20);

                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("DOB");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.EstimatedTime).HasMaxLength(50);

                entity.Property(e => e.FamilyContactName).HasMaxLength(100);

                entity.Property(e => e.FamilyContactNumber).HasMaxLength(50);

                entity.Property(e => e.Hpi)
                    .HasMaxLength(500)
                    .HasColumnName("HPI");

                entity.Property(e => e.IsEms).HasColumnName("IsEMS");

                entity.Property(e => e.LastKnownWell).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.PatientName).HasMaxLength(100);

                entity.Property(e => e.StartingPoint).HasMaxLength(50);

                entity.Property(e => e.Video).HasMaxLength(500);
            });

            modelBuilder.Entity<CodeStemi>(entity =>
            {
                entity.ToTable("CodeSTEMIs");

                entity.Property(e => e.CodeStemiid).HasColumnName("CodeSTEMIId");

                entity.Property(e => e.Attachments).HasMaxLength(500);

                entity.Property(e => e.Audio).HasMaxLength(500);

                entity.Property(e => e.BloodThinners).HasMaxLength(100);

                entity.Property(e => e.ChiefComplant).HasMaxLength(200);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Distance).HasMaxLength(20);

                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("DOB");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.EstimatedTime).HasMaxLength(50);

                entity.Property(e => e.FamilyContactName).HasMaxLength(100);

                entity.Property(e => e.FamilyContactNumber).HasMaxLength(50);

                entity.Property(e => e.Hpi)
                    .HasMaxLength(500)
                    .HasColumnName("HPI");

                entity.Property(e => e.IsEms).HasColumnName("IsEMS");

                entity.Property(e => e.LastKnownWell).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.PatientName).HasMaxLength(100);

                entity.Property(e => e.StartingPoint).HasMaxLength(50);

                entity.Property(e => e.Video).HasMaxLength(500);
            });

            modelBuilder.Entity<CodeStroke>(entity =>
            {
                entity.Property(e => e.Attachments).HasMaxLength(500);

                entity.Property(e => e.Audio).HasMaxLength(500);

                entity.Property(e => e.BloodThinners).HasMaxLength(100);

                entity.Property(e => e.ChiefComplant).HasMaxLength(200);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Distance).HasMaxLength(20);

                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("DOB");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.EstimatedTime).HasMaxLength(50);

                entity.Property(e => e.FamilyContactName).HasMaxLength(100);

                entity.Property(e => e.FamilyContactNumber).HasMaxLength(50);

                entity.Property(e => e.Hpi)
                    .HasMaxLength(500)
                    .HasColumnName("HPI");

                entity.Property(e => e.IsEms).HasColumnName("IsEMS");

                entity.Property(e => e.LastKnownWell).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.PatientName).HasMaxLength(100);

                entity.Property(e => e.StartingPoint).HasMaxLength(50);

                entity.Property(e => e.Video).HasMaxLength(500);
            });

            modelBuilder.Entity<CodeTrauma>(entity =>
            {
                entity.Property(e => e.Attachments).HasMaxLength(500);

                entity.Property(e => e.Audio).HasMaxLength(500);

                entity.Property(e => e.BloodThinners).HasMaxLength(100);

                entity.Property(e => e.ChiefComplant).HasMaxLength(200);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Distance).HasMaxLength(20);

                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("DOB");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.EstimatedTime).HasMaxLength(50);

                entity.Property(e => e.FamilyContactName).HasMaxLength(100);

                entity.Property(e => e.FamilyContactNumber).HasMaxLength(50);

                entity.Property(e => e.Hpi)
                    .HasMaxLength(500)
                    .HasColumnName("HPI");

                entity.Property(e => e.IsEms).HasColumnName("IsEMS");

                entity.Property(e => e.LastKnownWell).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.PatientName).HasMaxLength(100);

                entity.Property(e => e.StartingPoint).HasMaxLength(50);

                entity.Property(e => e.Video).HasMaxLength(500);
            });

            modelBuilder.Entity<CodesServiceLinesMapping>(entity =>
            {
                entity.ToTable("CodesServiceLinesMapping");

                entity.Property(e => e.ActiveCodeName)
                    .IsRequired()
                    .HasMaxLength(15);
            });

            modelBuilder.Entity<Component>(entity =>
            {
                entity.Property(e => e.ComModuleName)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.ModuleImage)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.PageDescription)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.PageName)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.PageTitle)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.PageUrl)
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ComponentAccess>(entity =>
            {
                entity.ToTable("ComponentAccess");

                entity.Property(e => e.ComponentIdFk).HasColumnName("ComponentIdFK");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.ComponentIdFkNavigation)
                    .WithMany(p => p.ComponentAccesses)
                    .HasForeignKey(d => d.ComponentIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Components_ComponentAccess");

                entity.HasOne(d => d.RoleIdFkNavigation)
                    .WithMany(p => p.ComponentAccesses)
                    .HasForeignKey(d => d.RoleIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ComponentAccess");
            });

            modelBuilder.Entity<Consult>(entity =>
            {
                entity.Property(e => e.CallbackNumber).HasMaxLength(15);

                entity.Property(e => e.ConsultType).HasMaxLength(50);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DateOfBirth).HasColumnType("datetime");

                entity.Property(e => e.Location).HasMaxLength(500);

                entity.Property(e => e.MedicalRecordNumber).HasMaxLength(100);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.PatientFirstName).HasMaxLength(50);

                entity.Property(e => e.PatientLastName).HasMaxLength(50);

                entity.HasOne(d => d.ServiceLineIdFkNavigation)
                    .WithMany(p => p.Consults)
                    .HasForeignKey(d => d.ServiceLineIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Consults_ServiceLine");
            });

            modelBuilder.Entity<ConsultAcknowledgment>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<ConsultField>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.FieldDataType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.FieldLabel).HasMaxLength(50);

                entity.Property(e => e.FieldName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.FieldType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<ControlList>(entity =>
            {
                entity.ToTable("ControlList");

                entity.Property(e => e.ControlListTitle)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ControlListType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<ControlListDetail>(entity =>
            {
                entity.ToTable("ControlListDetail");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.ImageHtml).IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.UniqueId)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.HasOne(d => d.ControlListIdFkNavigation)
                    .WithMany(p => p.ControlListDetails)
                    .HasForeignKey(d => d.ControlListIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ControlList_data");
            });

            modelBuilder.Entity<ConversationChannel>(entity =>
            {
                entity.Property(e => e.ChannelSid).HasMaxLength(256);

                entity.Property(e => e.ConversationImage).HasMaxLength(200);

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FriendlyName)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.UniqueName)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<ConversationParticipant>(entity =>
            {
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FriendlyName)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.UniqueName)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.HasOne(d => d.ConversationChannelIdFkNavigation)
                    .WithMany(p => p.ConversationParticipants)
                    .HasForeignKey(d => d.ConversationChannelIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChannelsChat_Users");

                entity.HasOne(d => d.UserIdFkNavigation)
                    .WithMany(p => p.ConversationParticipants)
                    .HasForeignKey(d => d.UserIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ChannelsMembersChat_Users");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DepartmentName).HasMaxLength(500);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.OrganizationIdFkNavigation)
                    .WithMany(p => p.Departments)
                    .HasForeignKey(d => d.OrganizationIdFk)
                    .HasConstraintName("FK_Departments_Organizations");
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

            modelBuilder.Entity<FavouriteTeam>(entity =>
            {
                entity.ToTable("FavouriteTeam");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.ServiceLineIdFkNavigation)
                    .WithMany(p => p.FavouriteTeams)
                    .HasForeignKey(d => d.ServiceLineIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FavouriteTeam_ServiceLine");

                entity.HasOne(d => d.UserIdFkNavigation)
                    .WithMany(p => p.FavouriteTeams)
                    .HasForeignKey(d => d.UserIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FavouriteTeam_Users");
            });

            modelBuilder.Entity<InteractiveVoiceResponse>(entity =>
            {
                entity.HasKey(e => e.IvrId)
                    .HasName("PK_dbo.IVResponse");

                entity.ToTable("InteractiveVoiceResponse");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Ivrsetting>(entity =>
            {
                entity.HasKey(e => e.IvrSettingsId)
                    .HasName("PK_dbo.InteractiveVoiceResponse");

                entity.ToTable("IVRSettings");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Icon).HasMaxLength(20);

                entity.Property(e => e.IvrparentId).HasColumnName("IVRParentId");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.IvrIdFkNavigation)
                    .WithMany(p => p.Ivrsettings)
                    .HasForeignKey(d => d.IvrIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_IVRSettings_InteractiveVoiceResponse");
            });

            modelBuilder.Entity<MdrouteCounter>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("MDRoute_Counter");

                entity.Property(e => e.CounterInitial)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("Counter_Initial");

                entity.Property(e => e.CounterName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("Counter_Name")
                    .HasDefaultValueSql("((10000))");

                entity.Property(e => e.CounterValue).HasColumnName("Counter_Value");
            });

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.Property(e => e.ActiveCodes).HasMaxLength(50);

                entity.Property(e => e.City).HasMaxLength(50);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.FaxNo).HasMaxLength(20);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.OrganizationName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.PhoneNo).HasMaxLength(15);

                entity.Property(e => e.PrimaryAddress).HasMaxLength(500);

                entity.Property(e => e.PrimaryAddress2).HasMaxLength(500);

                entity.Property(e => e.PrimaryMobileNo).HasMaxLength(15);

                entity.Property(e => e.PrimaryMobileNo2).HasMaxLength(15);

                entity.Property(e => e.Zip).HasMaxLength(100);
            });

            modelBuilder.Entity<OrganizationConsultField>(entity =>
            {
                entity.HasKey(e => e.OrgConsultFieldId);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.ConsultFieldIdFkNavigation)
                    .WithMany(p => p.OrganizationConsultFields)
                    .HasForeignKey(d => d.ConsultFieldIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrganizationConsultFields_ConsultFields");

                entity.HasOne(d => d.OrganizationIdFkNavigation)
                    .WithMany(p => p.OrganizationConsultFields)
                    .HasForeignKey(d => d.OrganizationIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OrganizationConsultFields_Organizations");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.RoleDiscrimination)
                    .HasMaxLength(128)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<ServiceLine>(entity =>
            {
                entity.ToTable("ServiceLine");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.ServiceName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(d => d.DepartmentIdFkNavigation)
                    .WithMany(p => p.ServiceLines)
                    .HasForeignKey(d => d.DepartmentIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Departments_ServiceLine");
            });

            modelBuilder.Entity<Setting>(entity =>
            {
                entity.HasIndex(e => e.OrganizationIdFk, "OrganizationIdFk_UK")
                    .IsUnique();

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.OrganizationEmail)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.OrganizationIdFkNavigation)
                    .WithOne(p => p.Setting)
                    .HasForeignKey<Setting>(d => d.OrganizationIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Settings_Organizations");
            });

            modelBuilder.Entity<State>(entity =>
            {
                entity.Property(e => e.StateId).ValueGeneratedNever();

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.StateName).HasMaxLength(500);

                entity.Property(e => e.StateProvince)
                    .HasMaxLength(100)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<Temp>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("temp");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.StartTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<TmpFerdeen>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("tmpFerdeen");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("date")
                    .HasColumnName("createdDate");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.City).HasMaxLength(40);

                entity.Property(e => e.CodeExpiryTime).HasColumnType("datetime");

                entity.Property(e => e.ConversationUserSid).HasMaxLength(50);

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Dob)
                    .HasColumnType("date")
                    .HasColumnName("DOB");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.Gender)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.HomeAddress)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Initials)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasDefaultValueSql("(' ')")
                    .IsFixedLength(true);

                entity.Property(e => e.IsActive).HasDefaultValueSql("((1))");

                entity.Property(e => e.LastName)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.MiddleName)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.OfficeAddress)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.OfficePhoneNumber).HasMaxLength(15);

                entity.Property(e => e.PersonalMobileNumber).HasMaxLength(15);

                entity.Property(e => e.PrimaryEmail).HasMaxLength(256);

                entity.Property(e => e.SecondaryEmail).HasMaxLength(256);

                entity.Property(e => e.StateKey).HasColumnName("State_key");

                entity.Property(e => e.TwoFactorCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TwoFactorExpiryDate).HasColumnType("datetime");

                entity.Property(e => e.UserChannelSid).HasMaxLength(50);

                entity.Property(e => e.UserImage)
                    .HasMaxLength(200)
                    .HasColumnName("User_Image");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.UserUniqueId).HasMaxLength(15);

                entity.Property(e => e.Zip).HasMaxLength(100);
            });

            modelBuilder.Entity<UserAccess>(entity =>
            {
                entity.ToTable("UserAccess");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.ComponentIdFkNavigation)
                    .WithMany(p => p.UserAccesses)
                    .HasForeignKey(d => d.ComponentIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Components_UserAccess");

                entity.HasOne(d => d.RoleIdFkNavigation)
                    .WithMany(p => p.UserAccesses)
                    .HasForeignKey(d => d.RoleIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserAccess_Users");

                entity.HasOne(d => d.UserIdFkNavigation)
                    .WithMany(p => p.UserAccesses)
                    .HasForeignKey(d => d.UserIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserAccess_Users1");
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasOne(d => d.RoleIdFkNavigation)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.RoleIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserRoles_Roles");

                entity.HasOne(d => d.UserIdFkNavigation)
                    .WithMany(p => p.UserRoles)
                    .HasForeignKey(d => d.UserIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserRoles_Users");
            });

            modelBuilder.Entity<UsersRelation>(entity =>
            {
                entity.ToTable("UsersRelation");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.ServiceLineIdFkNavigation)
                    .WithMany(p => p.UsersRelations)
                    .HasForeignKey(d => d.ServiceLineIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UsersRelation_ServiceLine");

                entity.HasOne(d => d.UserIdFkNavigation)
                    .WithMany(p => p.UsersRelations)
                    .HasForeignKey(d => d.UserIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UsersRelation_Users");
            });

            modelBuilder.Entity<UsersSchedule>(entity =>
            {
                entity.ToTable("UsersSchedule");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.ScheduleDate).HasColumnType("datetime");

                entity.Property(e => e.ScheduleDateEnd).HasColumnType("datetime");

                entity.Property(e => e.ScheduleDateStart).HasColumnType("datetime");

                entity.HasOne(d => d.RoleIdFkNavigation)
                    .WithMany(p => p.UsersSchedules)
                    .HasForeignKey(d => d.RoleIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UsersSchedule_Roles");

                entity.HasOne(d => d.ServiceLineIdFkNavigation)
                    .WithMany(p => p.UsersSchedules)
                    .HasForeignKey(d => d.ServiceLineIdFk)
                    .HasConstraintName("FK_UsersSchedule_Serviceline");

                entity.HasOne(d => d.UserIdFkNavigation)
                    .WithMany(p => p.UsersSchedules)
                    .HasForeignKey(d => d.UserIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UsersSchedule_Users");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
