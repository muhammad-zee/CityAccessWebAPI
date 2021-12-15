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

        public virtual DbSet<Component> Components { get; set; }
        public virtual DbSet<ComponentAccess> ComponentAccesses { get; set; }
        public virtual DbSet<ControlList> ControlLists { get; set; }
        public virtual DbSet<ControlListDetail> ControlListDetails { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<DepartmentService> DepartmentServices { get; set; }
        public virtual DbSet<ElmahError> ElmahErrors { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public virtual DbSet<OrganizationDepartment> OrganizationDepartments { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<ServiceLine> ServiceLines { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserAccess> UserAccesses { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=192.168.0.11;Initial Catalog=RouteAndQueue;User Id=sa;Password=4292;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

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

            modelBuilder.Entity<Department>(entity =>
            {
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DepartmentName).HasMaxLength(500);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<DepartmentService>(entity =>
            {
                entity.ToTable("DepartmentService");

                entity.HasOne(d => d.DepartmentIdFkNavigation)
                    .WithMany(p => p.DepartmentServices)
                    .HasForeignKey(d => d.DepartmentIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Department_Service");

                entity.HasOne(d => d.ServiceIdFkNavigation)
                    .WithMany(p => p.DepartmentServices)
                    .HasForeignKey(d => d.ServiceIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Department_Service1");
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

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.Property(e => e.City).HasMaxLength(50);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.FaxNo).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.OrganizationName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.PhoneNo).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.PrimaryAddress).HasMaxLength(50);

                entity.Property(e => e.PrimaryAddress2).HasMaxLength(50);

                entity.Property(e => e.PrimaryMobileNo).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.PrimaryMobileNo2).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Zip).HasMaxLength(100);
            });

            modelBuilder.Entity<OrganizationDepartment>(entity =>
            {
                entity.ToTable("OrganizationDepartment");

                entity.HasOne(d => d.DepartmentIdFkNavigation)
                    .WithMany(p => p.OrganizationDepartments)
                    .HasForeignKey(d => d.DepartmentIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Organization_Department1");

                entity.HasOne(d => d.OrganizationIdFkNavigation)
                    .WithMany(p => p.OrganizationDepartments)
                    .HasForeignKey(d => d.OrganizationIdFk)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Organization_Department");
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
                entity.HasKey(e => e.ServiceId)
                    .HasName("ServiceId");

                entity.ToTable("ServiceLine");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.ServiceName)
                    .IsRequired()
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<State>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.StateName).HasMaxLength(500);

                entity.Property(e => e.StateProvince)
                    .HasMaxLength(100)
                    .IsFixedLength(true);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.City).HasMaxLength(40);

                entity.Property(e => e.CodeExpiryTime).HasColumnType("datetime");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(256)
                    .IsUnicode(false);

                entity.Property(e => e.Gender)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.HomeAddress)
                    .HasMaxLength(500)
                    .IsUnicode(false);

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

                entity.Property(e => e.PrimaryEmail).HasMaxLength(256);

                entity.Property(e => e.SecondaryEmail).HasMaxLength(256);

                entity.Property(e => e.StateKey).HasColumnName("State_key");

                entity.Property(e => e.TwoFactorCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TwoFactorExpiryDate).HasColumnType("datetime");

                entity.Property(e => e.UserImage)
                    .HasMaxLength(200)
                    .HasColumnName("User_Image");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(256);

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

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
