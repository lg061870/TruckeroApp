using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Truckero.Core.Entities;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Tests.Helpers;

/// <summary>
/// Provides helper methods for ensuring entities exist before adding them in tests
/// </summary>
public static class EntityTestHelpers
{
    /// <summary>
    /// Ensures a user exists in the database before using it in tests
    /// </summary>
    public static async Task<User> EnsureUserExistsAsync(this AppDbContext dbContext, User user)
    {
        // Check by ID first
        var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        if (existingUser != null)
            return existingUser;

        // Then check by email as a fallback
        existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        if (existingUser != null)
            return existingUser;

        // User doesn't exist, add it
        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();
        
        // Create an onboarding record if it doesn't exist
        var onboarding = await dbContext.Onboardings.FirstOrDefaultAsync(o => o.UserId == user.Id);
        if (onboarding == null)
        {
            onboarding = new OnboardingProgress
            {
                UserId = user.Id,
                StepCurrent = "start",
                Completed = false,
                LastUpdated = DateTime.UtcNow
            };
            
            dbContext.Onboardings.Add(onboarding);
            await dbContext.SaveChangesAsync();
        }
        
        return user;
    }

    /// <summary>
    /// Ensures a truck exists in the database before using it in tests
    /// </summary>
    public static async Task<Truck> EnsureTruckExistsAsync(this AppDbContext dbContext, Truck truck)
    {
        // Check by ID
        var existingTruck = await dbContext.Trucks.FirstOrDefaultAsync(t => t.Id == truck.Id);
        if (existingTruck != null)
            return existingTruck;

        // Check by license plate as fallback (if license plate is unique)
        if (!string.IsNullOrEmpty(truck.LicensePlate))
        {
            existingTruck = await dbContext.Trucks.FirstOrDefaultAsync(t => t.LicensePlate == truck.LicensePlate);
            if (existingTruck != null)
                return existingTruck;
        }

        // Truck doesn't exist, add it
        await dbContext.Trucks.AddAsync(truck);
        await dbContext.SaveChangesAsync();
        return truck;
    }

    /// <summary>
    /// Ensures a payment method exists in the database before using it in tests
    /// </summary>
    public static async Task<PaymentMethod> EnsurePaymentMethodExistsAsync(this AppDbContext dbContext, PaymentMethod paymentMethod)
    {
        // Check by ID
        var existingPaymentMethod = await dbContext.PaymentMethods.FirstOrDefaultAsync(p => p.Id == paymentMethod.Id);
        if (existingPaymentMethod != null)
            return existingPaymentMethod;

        // Payment method doesn't exist, add it
        await dbContext.PaymentMethods.AddAsync(paymentMethod);
        await dbContext.SaveChangesAsync();
        return paymentMethod;
    }

    /// <summary>
    /// Ensures a payment method type exists in the database before using it in tests
    /// </summary>
    public static async Task<PaymentMethodType> EnsurePaymentMethodTypeExistsAsync(this AppDbContext dbContext, PaymentMethodType paymentMethodType)
    {
        // Check by ID
        var existingType = await dbContext.PaymentMethodTypes.FirstOrDefaultAsync(p => p.Id == paymentMethodType.Id);
        if (existingType != null)
            return existingType;

        // Check by name as fallback
        existingType = await dbContext.PaymentMethodTypes.FirstOrDefaultAsync(p => p.Name == paymentMethodType.Name);
        if (existingType != null)
            return existingType;

        // Payment method type doesn't exist, add it
        await dbContext.PaymentMethodTypes.AddAsync(paymentMethodType);
        await dbContext.SaveChangesAsync();
        return paymentMethodType;
    }

    /// <summary>
    /// Generic method to ensure any entity exists by ID
    /// </summary>
    public static async Task<T> EnsureEntityExistsAsync<T>(this AppDbContext dbContext, T entity, Func<T, object> idSelector) where T : class
    {
        var id = idSelector(entity);
        var entityType = dbContext.Model.FindEntityType(typeof(T));
        var idProperty = entityType?.FindPrimaryKey()?.Properties.FirstOrDefault();

        if (idProperty == null)
            throw new ArgumentException($"Could not find primary key for entity type {typeof(T).Name}");

        // Use EF Core's Find method which is optimized for primary key lookup
        var existingEntity = await dbContext.FindAsync<T>(id);
        if (existingEntity != null)
            return existingEntity;

        // Entity doesn't exist, add it
        await dbContext.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        return entity;
    }
}