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

        public virtual DbSet<Component> Components { get; set; }
        public virtual DbSet<ComponentAccess> ComponentAccesses { get; set; }
        public virtual DbSet<ControlList> ControlLists { get; set; }
        public virtual DbSet<ControlListDetail> ControlListDetails { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
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

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.RoleDiscrimination)
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(256);
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
