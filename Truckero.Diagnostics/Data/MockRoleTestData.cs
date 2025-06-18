using System;
using System.Collections.Generic;
using Truckero.Core.Entities;
using Truckero.Core.Enums;

namespace Truckero.Diagnostics.Data;

/// <summary>
/// Provides mock role data for unit tests
/// </summary>
public static class MockRoleTestData
{
    // These IDs match the seeded roles in AppDbContext.OnModelCreating
    public static class Ids
    {
        public static readonly Guid GuestRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public static readonly Guid CustomerRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        public static readonly Guid DriverRoleId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        public static readonly Guid StoreClerkRoleId = Guid.Parse("00000000-0000-0000-0000-000000000004");
        public static readonly Guid AdminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000005");
    }
    
    // Role objects matching seeded data in AppDbContext
    public static Role GuestRole => new()
    {
        Id = Ids.GuestRoleId,
        Name = RoleType.Guest.ToString()
    };
    
    public static Role CustomerRole => new()
    {
        Id = Ids.CustomerRoleId,
        Name = RoleType.Customer.ToString()
    };
    
    public static Role DriverRole => new()
    {
        Id = Ids.DriverRoleId,
        Name = RoleType.Driver.ToString()
    };
    
    public static Role StoreClerkRole => new()
    {
        Id = Ids.StoreClerkRoleId,
        Name = RoleType.StoreClerk.ToString()
    };
    
    public static Role AdminRole => new()
    {
        Id = Ids.AdminRoleId,
        Name = RoleType.Admin.ToString()
    };
    
    // Helper method to get all standard roles
    public static List<Role> GetAllRoles() => new()
    {
        GuestRole,
        CustomerRole,
        DriverRole,
        StoreClerkRole,
        AdminRole
    };
}