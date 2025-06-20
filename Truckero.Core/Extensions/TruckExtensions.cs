using Truckero.Core.DTOs.Trucks;
using Truckero.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Truckero.Core.DTOs.Onboarding;

namespace Truckero.Core.Extensions {
    public static class TruckExtensions {
        public static TruckRequest ToTruckRequestDto(this Truck truck) {
            return new TruckRequest {
                Id = truck.Id,
                DriverProfileId = truck.DriverProfileId,
                TruckTypeId = truck.TruckTypeId,
                TruckMakeId = truck.TruckMakeId,
                TruckModelId = truck.TruckModelId,
                LicensePlate = truck.LicensePlate ?? string.Empty,
                Year = truck.Year,
                PhotoFrontUrl = truck.PhotoFrontUrl,
                PhotoBackUrl = truck.PhotoBackUrl,
                PhotoLeftUrl = truck.PhotoLeftUrl,
                PhotoRightUrl = truck.PhotoRightUrl,
                TruckCategoryId = truck.TruckCategoryId,
                BedTypeId = truck.BedTypeId,
                UseTagIds = truck.UseTags?.Select(x => x.UseTagId).ToList() ?? new List<Guid>(),
                OwnershipType = truck.OwnershipType,
                InsuranceProvider = truck.InsuranceProvider ?? string.Empty,
                PolicyNumber = truck.PolicyNumber ?? string.Empty,
                InsuranceDocumentUrl = truck.InsuranceDocumentUrl,
                IsActive = truck.IsActive,
                IsVerified = truck.IsVerified
            };
        }

        public static Truck ToTruckEntity(this TruckRequest dto) {
            return new Truck {
                Id = dto.Id,
                DriverProfileId = dto.DriverProfileId,
                TruckTypeId = dto.TruckTypeId,
                TruckMakeId = dto.TruckMakeId,
                TruckModelId = dto.TruckModelId,
                LicensePlate = dto.LicensePlate,
                Year = dto.Year,
                PhotoFrontUrl = dto.PhotoFrontUrl,
                PhotoBackUrl = dto.PhotoBackUrl,
                PhotoLeftUrl = dto.PhotoLeftUrl,
                PhotoRightUrl = dto.PhotoRightUrl,
                TruckCategoryId = dto.TruckCategoryId,
                BedTypeId = dto.BedTypeId,
                OwnershipType = dto.OwnershipType,
                InsuranceProvider = dto.InsuranceProvider,
                PolicyNumber = dto.PolicyNumber,
                InsuranceDocumentUrl = dto.InsuranceDocumentUrl,
                IsActive = dto.IsActive,
                IsVerified = dto.IsVerified
                // UseTags/TruckUseTags handled separately
            };
        }
    }
}
