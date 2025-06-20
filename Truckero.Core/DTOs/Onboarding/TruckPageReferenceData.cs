using System.Collections.Generic;
using Truckero.Core.Entities;

namespace Truckero.Core.DTOs.Onboarding;

public class TruckReferenceData
{
    public List<TruckMake> TruckMakes { get; set; } = new();
    public List<TruckModel> TruckModels { get; set; } = new();
    public List<TruckCategory> TruckCategories { get; set; } = new();
    public List<BedType> BedTypes { get; set; } = new();
    public List<UseTag> UseTags { get; set; } = new();
    public List<TruckType> TruckTypes { get; set; } = new();
}
