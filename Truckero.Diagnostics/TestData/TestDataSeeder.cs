using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Truckero.Core.Entities;
using Truckero.Infrastructure.Data;

namespace Truckero.Diagnostics.TestData; 

public static class TestDataSeeder
{
    public static User SeedUser(AppDbContext context, string email = "test@example.com")
    {
        var user = new User { Id = Guid.NewGuid(), /* ... */ };
        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }

    public static CustomerProfile SeedCustomerProfile(AppDbContext context, User user)
    {
        var profile = new CustomerProfile { UserId = user.Id, /* ... */ };
        context.CustomerProfiles.Add(profile);
        context.SaveChanges();
        return profile;
    }

    // Add more methods for DriverProfile, Truck, etc.
}
