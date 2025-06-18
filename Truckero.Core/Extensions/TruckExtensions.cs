using Truckero.Core.DTOs.Trucks;
using Truckero.Core.Entities;
using System.Linq;

namespace Truckero.Core.Extensions; 
public static class TruckExtensions {
    public static TruckRequestDto ToTruckRequestDto(this Truck truck) {
        return new TruckRequestDto {
            Id = truck.Id,
            TruckTypeId = truck.TruckTypeId,
            TruckMakeId = truck.TruckMakeId,
            TruckModelId = truck.TruckModelId,
            LicensePlate = truck.LicensePlate ?? string.Empty,
            Year = truck.Year,
            PhotoFrontUrl = truck.PhotoFrontUrl,
            PhotoBackUrl = truck.PhotoBackUrl,
            PhotoLeftUrl = truck.PhotoLeftUrl,
            PhotoRightUrl = truck.PhotoRightUrl,
            LoadCategory = truck.LoadCategory,
            TruckCategoryId = truck.TruckCategoryId,
            BedTypeId = truck.BedTypeId,
            UseTagIds = truck.UseTags?.Select(t => t.UseTagId).ToList(),
            OwnershipType = truck.OwnershipType,
            InsuranceProvider = truck.InsuranceProvider ?? string.Empty,
            PolicyNumber = truck.PolicyNumber ?? string.Empty,
            InsuranceDocumentUrl = truck.InsuranceDocumentUrl,
        };
    }
}
