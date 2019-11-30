using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Lab8.Models.DataAccess
{
    public partial class StudentRecordContext : DbContext
    {
        public StudentRecordContext()
        {
        }

        public StudentRecordContext(DbContextOptions<StudentRecordContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AcademicRecord> AcademicRecord { get; set; }
        public virtual DbSet<Course> Course { get; set; }
        public virtual DbSet<Employee> Employee { get; set; }
        public virtual DbSet<EmployeeRole> EmployeeRole { get; set; }
        public virtual DbSet<Registration> Registration { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<Student> Student { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = Config.ConnectionString("StudentRecord");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AcademicRecord>(entity =>
            {
                entity.HasKey(e => new { e.StudentId, e.CourseCode })
                    .HasName("PK__Academic__3D052599DB7DA8FC");

                entity.Property(e => e.StudentId).HasMaxLength(16);

                entity.Property(e => e.CourseCode).HasMaxLength(16);

                entity.HasOne(d => d.CourseCodeNavigation)
                    .WithMany(p => p.AcademicRecord)
                    .HasForeignKey(d => d.CourseCode)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AcademicRecord_Course");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.AcademicRecord)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AcademicRecord_Student");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Code)
                    .HasName("PK__Course__A25C5AA6A5FB13AC");

                entity.Property(e => e.Code).HasMaxLength(16);

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.FeeBase).HasColumnType("decimal(6, 2)");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<EmployeeRole>(entity =>
            {
                entity.HasKey(e => new { e.EmployeeId, e.RoleId });

                entity.ToTable("Employee_Role");

                entity.Property(e => e.EmployeeId).HasColumnName("Employee_Id");

                entity.Property(e => e.RoleId).HasColumnName("Role_Id");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.EmployeeRole)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ToEmployee");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.EmployeeRole)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ToRole");
            });

            modelBuilder.Entity<Registration>(entity =>
            {
                entity.HasKey(e => new { e.CourseCourseId, e.StudentStudentNum })
                    .HasName("PK__Registra__92ECCCE92BFF96DF");

                entity.Property(e => e.CourseCourseId)
                    .HasColumnName("Course_CourseID")
                    .HasMaxLength(16);

                entity.Property(e => e.StudentStudentNum)
                    .HasColumnName("Student_StudentNum")
                    .HasMaxLength(16);

                entity.HasOne(d => d.CourseCourse)
                    .WithMany(p => p.Registration)
                    .HasForeignKey(d => d.CourseCourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Registration_ToCourse");

                entity.HasOne(d => d.StudentStudentNumNavigation)
                    .WithMany(p => p.Registration)
                    .HasForeignKey(d => d.StudentStudentNum)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Registration_ToStudent");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Role1)
                    .HasColumnName("Role")
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(16);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
