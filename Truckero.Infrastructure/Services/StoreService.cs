using Microsoft.EntityFrameworkCore;
using Truckero.Core.Constants;
using Truckero.Core.DTOs;
using Truckero.Core.DTOs.Store;
using Truckero.Core.Entities;
using Truckero.Core.Exceptions;
using Truckero.Core.Interfaces.Repositories;
using Truckero.Core.Interfaces.Services;

namespace Truckero.Infrastructure.Services;

public class StoreService : IStoreService
{
    private readonly IStoreRepository _storeRepository;

    public StoreService(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    public async Task<StoreResponse> CreateStoreAsync(StoreRequest request)
    {
        // Basic FK check to give cleaner errors than raw DbUpdateException
        var companyExists = await _storeRepository.CompanyExistsAsync(request.CompanyId);
        if (!companyExists)
        {
            throw new ReferentialIntegrityException(ExceptionCodes.StoreErrorCodes.ForeignKeyNotFound,
                $"Company with id {request.CompanyId} not found");
        }

        var store = new Store
        {
            Id = Guid.NewGuid(),
            CompanyId = request.CompanyId,
            Name = request.Name,
            LegalEntity = request.LegalEntity,
            DomainWhitelist = request.DomainWhitelist,
            StoreType = request.StoreType,
            ParentStoreId = request.ParentStoreId,
            Region = request.Region,
            Address = request.Address,
            CreatedAt = DateTime.UtcNow,
            IsAuthorized = false
        };

        await _storeRepository.AddAsync(store);

        try
        {
            await _storeRepository.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new StoreException(ExceptionCodes.StoreErrorCodes.SaveFailed, ex.Message);
        }

        return new StoreResponse
        {
            Success = true,
            Store = MapToDto(store)
        };
    }

    public async Task<StoreResponse> UpdateStoreAsync(Guid id, StoreRequest request)
    {
        var store = await _storeRepository.GetByIdAsync(id);
        if (store == null)
        {
            return new StoreResponse
            {
                Success = false,
                ErrorCode = ExceptionCodes.StoreErrorCodes.NotFound,
                Message = $"Store with id {id} not found"
            };
        }

        if (store.CompanyId != request.CompanyId)
        {
            // Prevent re-parenting to another company without explicit design
            throw new StoreException(ExceptionCodes.IdMismatch,
                "Cannot change store's company via update");
        }

        store.Name = request.Name;
        store.LegalEntity = request.LegalEntity;
        store.DomainWhitelist = request.DomainWhitelist;
        store.StoreType = request.StoreType;
        store.ParentStoreId = request.ParentStoreId;
        store.Region = request.Region;
        store.Address = request.Address;
        store.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _storeRepository.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new StoreException(ExceptionCodes.StoreErrorCodes.SaveFailed, ex.Message);
        }

        return new StoreResponse
        {
            Success = true,
            Store = MapToDto(store)
        };
    }

    public async Task<BaseResponse> DeleteStoreAsync(Guid id)
    {
        var store = await _storeRepository.GetByIdAsync(id);
        if (store == null)
        {
            return new BaseResponse
            {
                Success = false,
                ErrorCode = ExceptionCodes.StoreErrorCodes.NotFound,
                Message = $"Store with id {id} not found"
            };
        }

        await _storeRepository.RemoveAsync(store);

        try
        {
            await _storeRepository.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new StoreException(ExceptionCodes.StoreErrorCodes.SaveFailed, ex.Message);
        }

        return new BaseResponse
        {
            Success = true
        };
    }

    public async Task<StoreResponse> GetStoreByIdAsync(Guid id)
    {
        var store = await _storeRepository.GetByIdAsync(id);
        if (store == null)
        {
            return new StoreResponse
            {
                Success = false,
                ErrorCode = ExceptionCodes.StoreErrorCodes.NotFound,
                Message = $"Store with id {id} not found"
            };
        }

        return new StoreResponse
        {
            Success = true,
            Store = MapToDto(store)
        };
    }

    public async Task<StoreListResponse> GetStoresByCompanyIdAsync(Guid companyId)
    {
        var stores = await _storeRepository.GetByCompanyIdAsync(companyId);

        return new StoreListResponse
        {
            Success = true,
            Stores = stores.Select(MapToDto).ToList()
        };
    }

    private static StoreRequest MapToDto(Store store) => new()
    {
        Id = store.Id,
        CompanyId = store.CompanyId,
        Name = store.Name,
        LegalEntity = store.LegalEntity,
        DomainWhitelist = store.DomainWhitelist,
        StoreType = store.StoreType,
        ParentStoreId = store.ParentStoreId,
        Region = store.Region,
        Address = store.Address
    };
}
