using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Starter_App.src.backend.Models;

public partial class AppStarterContext : IdentityDbContext<AspNetUser, AspNetRole, string>
{
    public AppStarterContext()
    {
    }

    public AppStarterContext(DbContextOptions<AppStarterContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);  // Calls Identity's configuration
        
        // Configure composite keys for Identity relationship tables
        modelBuilder.Entity<AspNetUserLogin>()
            .HasKey(ul => new { ul.LoginProvider, ul.ProviderKey });
            
        modelBuilder.Entity<AspNetUserToken>()
            .HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StudentId)
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("StudentId");
        });
    }
}