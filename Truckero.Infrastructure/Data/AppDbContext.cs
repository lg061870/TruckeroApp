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
    public DbSet<ConfirmationToken> ConfirmationTokens => Set<ConfirmationToken>();

    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<TruckType> TruckTypes => Set<TruckType>();
    public DbSet<PaymentMethodType> PaymentMethodTypes => Set<PaymentMethodType>();
    public DbSet<CustomerProfile> CustomerProfiles => Set<CustomerProfile>();
    public DbSet<Truck> Trucks { get; set; }
    public DbSet<UseTag> UseTags { get; set; }
    public DbSet<TruckUseTag> TruckUseTags { get; set; }
    public DbSet<TruckMake> TruckMakes { get; set; }
    public DbSet<TruckModel> TruckModels { get; set; }
    public DbSet<TruckCategory> TruckCategories { get; set; }
    public DbSet<BedType> BedTypes { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Bank> Banks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 🧩 Enum-to-string mappings (only for actual enums)
        modelBuilder.Entity<Role>()
            .Property(r => r.Name)
            .HasConversion<string>();

        // Map OwnershipType enum as string
        modelBuilder.Entity<Truck>()
            .Property(t => t.OwnershipType)
            .HasConversion<string>();

        // 🧷 Relationships & Composite Keys
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .IsRequired(true); // Explicitly mark this foreign key as required

        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .ValueGeneratedNever(); // <-- Ensures EF won't expect DB to assign this

        // 📧 Ensure unique email for login and registration
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();


        modelBuilder.Entity<OnboardingProgress>()
            .HasKey(o => o.UserId);

        // 🚛 DriverProfile Configuration
        modelBuilder.Entity<DriverProfile>()
            .HasKey(d => d.UserId);

        modelBuilder.Entity<DriverProfile>()
            .HasOne(d => d.User)
            .WithOne(u => u.DriverProfile)
            .HasForeignKey<DriverProfile>(d => d.UserId);

        modelBuilder.Entity<DriverProfile>()
            .Property(dp => dp.HomeBase)
            .IsRequired();

        modelBuilder.Entity<DriverProfile>()
            .Property(dp => dp.ServiceRadiusKm)
            .HasDefaultValue(25);


        // 🛒 CustomerProfile Configuration
        modelBuilder.Entity<CustomerProfile>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<CustomerProfile>()
            .HasOne(c => c.User)
            .WithOne(u => u.CustomerProfile)
            .HasForeignKey<CustomerProfile>(c => c.UserId);


        // StoreClerkProfile Configuration
        modelBuilder.Entity<StoreClerkProfile>()
            .HasKey(scp => scp.Id);

        modelBuilder.Entity<StoreClerkProfile>()
            .HasOne(scp => scp.User)
            .WithOne(u => u.StoreClerkProfile)
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

        modelBuilder.Entity<Truck>()
            .HasKey(t => t.Id);

        modelBuilder.Entity<Truck>()
            .HasOne(v => v.TruckType)
            .WithMany(vt => vt.Vehicles)
            .HasForeignKey(v => v.TruckTypeId);

        modelBuilder.Entity<Truck>()
            .HasOne(v => v.DriverProfile)
            .WithMany(dp => dp.Trucks)
            .HasForeignKey(v => v.DriverProfileId);

        modelBuilder.Entity<Truck>()
            .HasOne(t => t.TruckMake)
            .WithMany()
            .HasForeignKey(t => t.TruckMakeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Truck>()
            .HasOne(t => t.TruckModel)
            .WithMany()
            .HasForeignKey(t => t.TruckModelId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Truck>()
            .HasOne(t => t.TruckType)
            .WithMany()
            .HasForeignKey(t => t.TruckTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Truck>()
            .HasOne(t => t.TruckCategory)
            .WithMany()
            .HasForeignKey(t => t.TruckCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Truck>()
            .HasOne(t => t.BedTypeNav)
            .WithMany()
            .HasForeignKey(t => t.BedTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure many-to-many relationship between Truck and UseTag
        modelBuilder.Entity<TruckUseTag>()
            .HasKey(tut => new { tut.TruckId, tut.UseTagId });

        modelBuilder.Entity<TruckUseTag>()
            .HasOne(tut => tut.Truck)
            .WithMany(t => t.UseTags)
            .HasForeignKey(tut => tut.TruckId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TruckUseTag>()
            .HasOne(tut => tut.UseTag)
            .WithMany(ut => ut.TruckUseTags)
            .HasForeignKey(tut => tut.UseTagId)
            .OnDelete(DeleteBehavior.Cascade);

        // TruckMake/TruckModel relationship
        modelBuilder.Entity<TruckModel>()
            .HasOne(m => m.Make)
            .WithMany(mk => mk.Models)
            .HasForeignKey(m => m.MakeId);

        // Country & Bank
        modelBuilder.Entity<Country>()
            .HasKey(c => c.Code);
        modelBuilder.Entity<Country>()
            .Property(c => c.Code)
            .HasMaxLength(2);
        modelBuilder.Entity<Country>()
            .Property(c => c.Name)
            .HasMaxLength(100);
        modelBuilder.Entity<Country>()
            .HasMany(c => c.Banks)
            .WithOne(b => b.Country)
            .HasForeignKey(b => b.CountryCode);
        modelBuilder.Entity<Bank>()
            .HasKey(b => b.Id);
        modelBuilder.Entity<Bank>()
            .Property(b => b.Name)
            .HasMaxLength(100);
        modelBuilder.Entity<Bank>()
            .Property(b => b.SwiftCode)
            .HasMaxLength(11);
        modelBuilder.Entity<Bank>()
            .Property(b => b.CountryCode)
            .HasMaxLength(2);
        modelBuilder.Entity<Bank>()
            .Property(b => b.BankCode)
            .HasMaxLength(20);
        modelBuilder.Entity<Bank>()
            .Property(b => b.IbanPrefix)
            .HasMaxLength(34);

        // 🌱 Seed initial roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = RoleType.Guest.ToString() },
            new Role { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = RoleType.Customer.ToString() },
            new Role { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Name = RoleType.Driver.ToString() },
            new Role { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), Name = RoleType.StoreClerk.ToString() },
            new Role { Id = Guid.Parse("00000000-0000-0000-0000-000000000005"), Name = RoleType.Admin.ToString() }
        );

        modelBuilder.Entity<TruckType>().HasData(
            new TruckType { Id = Guid.Parse("00000000-0000-0000-0000-000000000101"), Name = "Motorcycle" },
            new TruckType { Id = Guid.Parse("00000000-0000-0000-0000-000000000102"), Name = "Sedan" },
            new TruckType { Id = Guid.Parse("00000000-0000-0000-0000-000000000103"), Name = "SUV" },
            new TruckType { Id = Guid.Parse("00000000-0000-0000-0000-000000000104"), Name = "Pickup" },
            new TruckType { Id = Guid.Parse("00000000-0000-0000-0000-000000000105"), Name = "Cargo Van" },
            new TruckType { Id = Guid.Parse("00000000-0000-0000-0000-000000000106"), Name = "Box Truck" },
            new TruckType { Id = Guid.Parse("00000000-0000-0000-0000-000000000107"), Name = "Flatbed" },
            new TruckType { Id = Guid.Parse("00000000-0000-0000-0000-000000000108"), Name = "Trailer" }
        );

        modelBuilder.Entity<PaymentMethodType>().HasData(
            new PaymentMethodType
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000301"),
                Name = "Card",
                Description = "Credit or debit card",
                IsForPayment = true,
                IsForPayout = false
            },
            new PaymentMethodType
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000302"),
                Name = "Wallet",
                Description = "Mobile wallet",
                IsForPayment = true,
                IsForPayout = false
            },
            new PaymentMethodType
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000303"),
                Name = "PayPal",
                Description = "PayPal account",
                IsForPayment = true,
                IsForPayout = true
            },
            new PaymentMethodType
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000304"),
                Name = "Bank",
                Description = "Bank transfer",
                IsForPayment = false,
                IsForPayout = true
            },
            new PaymentMethodType
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000305"),
                Name = "Cash",
                Description = "Cash on delivery",
                IsForPayment = true,
                IsForPayout = false
            },
            new PaymentMethodType
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000306"),
                Name = "Crypto",
                Description = "Cryptocurrency payment",
                IsForPayment = true,
                IsForPayout = false
            }
        );

        modelBuilder.Entity<UseTag>().HasData(
            new UseTag { Id = Guid.Parse("00000000-0000-0000-0000-000000000201"), Name = "Furniture Move" },
            new UseTag { Id = Guid.Parse("00000000-0000-0000-0000-000000000202"), Name = "Appliance Haul" },
            new UseTag { Id = Guid.Parse("00000000-0000-0000-0000-000000000203"), Name = "Store Delivery" },
            new UseTag { Id = Guid.Parse("00000000-0000-0000-0000-000000000204"), Name = "Junk Removal" },
            new UseTag { Id = Guid.Parse("00000000-0000-0000-0000-000000000205"), Name = "Fragile Goods" },
            new UseTag { Id = Guid.Parse("00000000-0000-0000-0000-000000000206"), Name = "Helper Included" }
        );

        // Seed TruckMakes
        modelBuilder.Entity<TruckMake>().HasData(
            new TruckMake { Id = Guid.Parse("00000000-0000-0000-0000-000000000301"), Name = "Ford" },
            new TruckMake { Id = Guid.Parse("00000000-0000-0000-0000-000000000302"), Name = "Toyota" },
            new TruckMake { Id = Guid.Parse("00000000-0000-0000-0000-000000000303"), Name = "Chevrolet" },
            new TruckMake { Id = Guid.Parse("00000000-0000-0000-0000-000000000304"), Name = "Dodge" },
            new TruckMake { Id = Guid.Parse("00000000-0000-0000-0000-000000000305"), Name = "GMC" },
            new TruckMake { Id = Guid.Parse("00000000-0000-0000-0000-000000000306"), Name = "RAM" },
            new TruckMake { Id = Guid.Parse("00000000-0000-0000-0000-000000000307"), Name = "Nissan" },
            new TruckMake { Id = Guid.Parse("00000000-0000-0000-0000-000000000308"), Name = "Other" }
        );

        // Seed TruckModels
        modelBuilder.Entity<TruckModel>().HasData(
            new TruckModel { Id = Guid.Parse("00000000-0000-0000-0000-000000000401"), Name = "F-150", MakeId = Guid.Parse("00000000-0000-0000-0000-000000000301") },
            new TruckModel { Id = Guid.Parse("00000000-0000-0000-0000-000000000402"), Name = "Tacoma", MakeId = Guid.Parse("00000000-0000-0000-0000-000000000302") },
            new TruckModel { Id = Guid.Parse("00000000-0000-0000-0000-000000000403"), Name = "Silverado", MakeId = Guid.Parse("00000000-0000-0000-0000-000000000303") },
            new TruckModel { Id = Guid.Parse("00000000-0000-0000-0000-000000000404"), Name = "RAM 1500", MakeId = Guid.Parse("00000000-0000-0000-0000-000000000306") },
            new TruckModel { Id = Guid.Parse("00000000-0000-0000-0000-000000000405"), Name = "Sierra", MakeId = Guid.Parse("00000000-0000-0000-0000-000000000305") },
            new TruckModel { Id = Guid.Parse("00000000-0000-0000-0000-000000000406"), Name = "Frontier", MakeId = Guid.Parse("00000000-0000-0000-0000-000000000307") },
            new TruckModel { Id = Guid.Parse("00000000-0000-0000-0000-000000000407"), Name = "Other", MakeId = Guid.Parse("00000000-0000-0000-0000-000000000308") }
        );

        // Seed TruckCategories
        modelBuilder.Entity<TruckCategory>().HasData(
            new TruckCategory { Id = Guid.Parse("00000000-0000-0000-0000-000000000501"), Name = "Small Load" },
            new TruckCategory { Id = Guid.Parse("00000000-0000-0000-0000-000000000502"), Name = "Standard Load" },
            new TruckCategory { Id = Guid.Parse("00000000-0000-0000-0000-000000000503"), Name = "Heavy Load" },
            new TruckCategory { Id = Guid.Parse("00000000-0000-0000-0000-000000000504"), Name = "Extra Heavy Duty" }
        );

        // Seed BedTypes
        modelBuilder.Entity<BedType>().HasData(
            new BedType { Id = Guid.Parse("00000000-0000-0000-0000-000000000601"), Name = "Open Bed" },
            new BedType { Id = Guid.Parse("00000000-0000-0000-0000-000000000602"), Name = "Covered Bed" },
            new BedType { Id = Guid.Parse("00000000-0000-0000-0000-000000000603"), Name = "Box Truck/Van" },
            new BedType { Id = Guid.Parse("00000000-0000-0000-0000-000000000604"), Name = "Refrigerated" }
        );

        // Seed Costa Rica and banks
        modelBuilder.Entity<Country>().HasData(
            new Country { Code = "CR", Name = "Costa Rica" }
        );
        modelBuilder.Entity<Bank>().HasData(
            new Bank { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Name = "Banco Nacional de Costa Rica", SwiftCode = "BNCRCRSJ", CountryCode = "CR", BankCode = "151", IbanPrefix = "CR" },
            new Bank { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Name = "Banco de Costa Rica", SwiftCode = "BCRICRSJ", CountryCode = "CR", BankCode = "152", IbanPrefix = "CR" },
            new Bank { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Name = "BAC Credomatic", SwiftCode = "BCCRCRSJ", CountryCode = "CR", BankCode = "254", IbanPrefix = "CR" }
        );
    }
}
