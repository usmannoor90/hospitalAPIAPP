using hospitalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace hospitalAPI.EFData;

public class EFDateContext : DbContext
{
    public EFDateContext(DbContextOptions<EFDateContext> options)
        : base(options)
    {

    }

    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    base.OnModelCreating(modelBuilder);
    //}

    protected override void OnModelCreating(ModelBuilder mB)
    {
        mB.Entity<Role>(e =>
        {
            e.Property(r => r.RoleName)
              .HasConversion<string>();

            e.HasData(
    new Role { id = 1, RoleName = RoleType.Admin },
    new Role { id = 2, RoleName = RoleType.Doctor },
    new Role { id = 3, RoleName = RoleType.Nurse },
    new Role { id = 4, RoleName = RoleType.Client }
);
        });

        mB.Entity<User>(ent =>
        {
            ent.HasKey(e => e.id);
            ent.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        mB.Entity<Appoinment>(entity =>
        {
            entity.HasKey(e => e.id);

            entity.HasOne(e => e.client)
                  .WithMany(c => c.appoinments)
                  .HasForeignKey(e => e.clientId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.doctor)
                  .WithMany(d => d.appoinments)
                  .HasForeignKey(e => e.doctorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.nurse)
                  .WithMany(n => n.appoinments)
                  .HasForeignKey(e => e.nurseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }




    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Nurse> Nurses { get; set; }
    public DbSet<Appoinment> Appoinments { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<MedicalBilling> MedicalBillings { get; set; }
}


