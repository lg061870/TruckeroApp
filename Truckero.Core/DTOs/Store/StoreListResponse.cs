using Truckero.Core.DTOs;

namespace Truckero.Core.DTOs;

public class StoreListResponse : BaseResponse
{
    public List<StoreRequest> Stores { get; set; } = new();
}
