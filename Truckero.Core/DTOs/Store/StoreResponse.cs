using Truckero.Core.DTOs;

namespace Truckero.Core.DTOs;

public class StoreResponse : BaseResponse
{
    public StoreRequest? Store { get; set; }
}
