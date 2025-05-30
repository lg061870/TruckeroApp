﻿using Microsoft.EntityFrameworkCore;
using Truckero.Core.Entities;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Infrastructure.Data;

namespace Truckero.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerProfile?> GetByUserIdAsync(Guid userId)
    {
        return await _context.CustomerProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task AddAsync(CustomerProfile profile)
    {
        await _context.CustomerProfiles.AddAsync(profile);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}