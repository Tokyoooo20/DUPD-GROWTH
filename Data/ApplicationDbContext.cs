using DupdGrowth.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DupdGrowth.Web.Data;

public class ApplicationDbContext : DbContext
{
    private readonly UserTableColumnOptions _userCols;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IOptions<UserTableColumnOptions> userColumns)
        : base(options)
    {
        _userCols = userColumns.Value;
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Office> Offices => Set<Office>();

    public DbSet<Pap> Paps => Set<Pap>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pap>(e =>
        {
            e.ToTable("paps");
            e.Property(p => p.Id).HasColumnName("pap_id");
            e.Property(p => p.PriorityNo).HasColumnName("priority_no");
            e.Property(p => p.PapName).HasColumnName("pap_name").IsRequired().HasMaxLength(200);
            e.Property(p => p.ResponsiblePerson).HasColumnName("responsible_person").HasMaxLength(200);
            e.Property(p => p.Budget).HasColumnName("budget").HasPrecision(18, 2);
            e.Property(p => p.TimeFrameStart).HasColumnName("time_frame_start");
            e.Property(p => p.TimeFrameEnd).HasColumnName("time_frame_end");
            e.Property(p => p.SupportOffice).HasColumnName("support_office").HasMaxLength(100);
            e.Property(p => p.AlignmentGrowth).HasColumnName("alignment_growth").HasMaxLength(100);
            e.Property(p => p.AlignmentAchieve).HasColumnName("alignment_achieve").HasMaxLength(200);
            e.Property(p => p.RemarksType).HasColumnName("remarks_type").HasMaxLength(50);
            e.Property(p => p.RemarksTypeOther).HasColumnName("remarks_type_other").HasMaxLength(200);
            e.Property(p => p.Remarks).HasColumnName("remarks").HasMaxLength(2000);
            e.Property(p => p.Status).HasColumnName("status").HasMaxLength(50);
            e.Property(p => p.CreatedByUserId).HasColumnName("created_by_user_id");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");

            e.HasOne(p => p.CreatedByUser)
                .WithMany()
                .HasForeignKey(p => p.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Office>(e =>
        {
            e.ToTable("offices");
            e.Property(o => o.Id).HasColumnName("office_id");
            e.Property(o => o.Name).HasColumnName("office_name").IsRequired().HasMaxLength(200);
            e.Property(o => o.ParentId).HasColumnName("parent_id");
        });

        var c = _userCols;
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable(c.Table);
            e.Property(u => u.Id).HasColumnName(c.Id);
            e.Property(u => u.Email).HasColumnName(c.Email).HasMaxLength(150);
            e.Property(u => u.Name).HasColumnName(c.Name).HasMaxLength(200);
            e.Property(u => u.ParentOfficeId).HasColumnName(c.ParentOfficeId);
            e.Property(u => u.OfficeId).HasColumnName(c.OfficeId);
            e.Property(u => u.PasswordHash).HasColumnName(c.PasswordHash).HasMaxLength(255);
            e.Property(u => u.CreatedAt).HasColumnName(c.CreatedAt);
            e.Property(u => u.IsApproved).HasColumnName(c.IsApproved);
            e.HasIndex(u => u.Email).IsUnique();

            e.HasOne(u => u.ParentOffice)
                .WithMany()
                .HasForeignKey(u => u.ParentOfficeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(u => u.Office)
                .WithMany()
                .HasForeignKey(u => u.OfficeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
