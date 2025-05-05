using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Enums;

namespace Truckero.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // 🚚 Core Tables
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AuthToken> AuthTokens => Set<AuthToken>();
    public DbSet<OnboardingProgress> Onboardings => Set<OnboardingProgress>();
    public DbSet<DriverProfile> DriverProfiles => Set<DriverProfile>();
    public DbSet<PayoutAccount> PayoutAccounts => Set<PayoutAccount>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<StoreClerkProfile> StoreClerkProfiles => Set<StoreClerkProfile>();
    public DbSet<StoreClerkAssignment> StoreClerkAssignments => Set<StoreClerkAssignment>();

    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleType> VehicleTypes => Set<VehicleType>();
    public DbSet<PaymentMethodType> PaymentMethodTypes => Set<PaymentMethodType>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 🧩 Enum-to-string mappings (only for actual enums)
        modelBuilder.Entity<Role>()
            .Property(r => r.Name)
            .HasConversion<string>();

        // 🧷 Relationships & Composite Keys
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId);

        modelBuilder.Entity<OnboardingProgress>()
            .HasKey(o => o.UserId);

        // 🚛 DriverProfile Configuration
        modelBuilder.Entity<DriverProfile>()
            .HasKey(d => d.UserId);

        modelBuilder.Entity<DriverProfile>()
            .HasOne(d => d.User)
            .WithOne(u => u.DriverProfile)
            .HasForeignKey<DriverProfile>(d => d.UserId);

        // 🛒 CustomerProfile Configuration
        modelBuilder.Entity<CustomerProfile>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<CustomerProfile>()
            .HasOne(c => c.User)
            .WithOne(u => u.CustomerProfile)
            .HasForeignKey<CustomerProfile>(c => c.UserId);


        // StoreClerkProfile Configuration
        modelBuilder.Entity<StoreClerkProfile>()
            .HasKey(scp => scp.UserId);

        modelBuilder.Entity<StoreClerkProfile>()
            .HasOne(scp => scp.User)
            .WithOne()
            .HasForeignKey<StoreClerkProfile>(scp => scp.UserId);

        // StoreClerkAssignment (many-to-many between ClerkProfile and Store)
        modelBuilder.Entity<StoreClerkAssignment>()
            .HasKey(sca => new { sca.ClerkUserId, sca.StoreId });

        modelBuilder.Entity<StoreClerkAssignment>()
            .HasOne(sca => sca.ClerkProfile)
            .WithMany(cp => cp.StoreAssignments)
            .HasForeignKey(sca => sca.ClerkUserId);

        modelBuilder.Entity<StoreClerkAssignment>()
            .HasOne(sca => sca.Store)
            .WithMany(s => s.Clerks)
            .HasForeignKey(sca => sca.StoreId);


        modelBuilder.Entity<AuthToken>()
            .HasOne(t => t.User)
            .WithMany(u => u.AuthTokens)
            .HasForeignKey(t => t.UserId);

        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId);

        modelBuilder.Entity<PaymentMethod>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<PaymentMethod>()
            .HasOne(p => p.User)
            .WithMany(u => u.PaymentMethods)
            .HasForeignKey(p => p.UserId);

        modelBuilder.Entity<PaymentMethod>()
            .HasOne(p => p.PaymentMethodType)
            .WithMany(pmt => pmt.PaymentMethods)
            .HasForeignKey(p => p.PaymentMethodTypeId);

        modelBuilder.Entity<PayoutAccount>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<PayoutAccount>()
            .HasOne(p => p.User)
            .WithMany(u => u.PayoutAccounts)
            .HasForeignKey(p => p.UserId);

        modelBuilder.Entity<PayoutAccount>()
            .HasOne(p => p.PaymentMethodType)
            .WithMany()
            .HasForeignKey(p => p.PaymentMethodTypeId);

        modelBuilder.Entity<SystemSetting>()
            .HasKey(s => s.Key);

        modelBuilder.Entity<Vehicle>()
            .HasKey(v => v.Id);

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.VehicleType)
            .WithMany(vt => vt.Vehicles)
            .HasForeignKey(v => v.VehicleTypeId);

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.DriverProfile)
            .WithMany(dp => dp.Vehicles)
            .HasForeignKey(v => v.DriverProfileId);


        // 🌱 Seed initial roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = RoleType.Guest },
            new Role { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = RoleType.Customer },
            new Role { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Name = RoleType.Driver },
            new Role { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), Name = RoleType.StoreClerk },
            new Role { Id = Guid.Parse("00000000-0000-0000-0000-000000000005"), Name = RoleType.Admin }
        );

        modelBuilder.Entity<VehicleType>().HasData(
            new VehicleType { Id = Guid.Parse("00000000-0000-0000-0000-000000000101"), Name = "Motorcycle" },
            new VehicleType { Id = Guid.Parse("00000000-0000-0000-0000-000000000102"), Name = "Sedan" },
            new VehicleType { Id = Guid.Parse("00000000-0000-0000-0000-000000000103"), Name = "SUV" },
            new VehicleType { Id = Guid.Parse("00000000-0000-0000-0000-000000000104"), Name = "Pickup" },
            new VehicleType { Id = Guid.Parse("00000000-0000-0000-0000-000000000105"), Name = "Cargo Van" },
            new VehicleType { Id = Guid.Parse("00000000-0000-0000-0000-000000000106"), Name = "Box Truck" },
            new VehicleType { Id = Guid.Parse("00000000-0000-0000-0000-000000000107"), Name = "Flatbed" },
            new VehicleType { Id = Guid.Parse("00000000-0000-0000-0000-000000000108"), Name = "Trailer" }
        );

        modelBuilder.Entity<PaymentMethodType>().HasData(
            new PaymentMethodType { Id = Guid.Parse("00000000-0000-0000-0000-000000000301"), Name = "Card" },
            new PaymentMethodType { Id = Guid.Parse("00000000-0000-0000-0000-000000000302"), Name = "Wallet" },
            new PaymentMethodType { Id = Guid.Parse("00000000-0000-0000-0000-000000000303"), Name = "PayPal" },
            new PaymentMethodType { Id = Guid.Parse("00000000-0000-0000-0000-000000000304"), Name = "Bank" },
            new PaymentMethodType { Id = Guid.Parse("00000000-0000-0000-0000-000000000305"), Name = "Cash" },
            new PaymentMethodType { Id = Guid.Parse("00000000-0000-0000-0000-000000000306"), Name = "Crypto" }
        );
    }
}
