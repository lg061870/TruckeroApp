namespace Truckero.Core.DTOs;

public class BusinessTypeResponse : BaseResponse
{
    public List<BusinessTypeRequest> BusinessTypes { get; set; } = new();
}
