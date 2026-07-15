using Truckero.Store.Models;

namespace Truckero.Store.Services;

public sealed class StoreSessionState
{
    private readonly List<StoreClerkAccountDraft> _createdClerks = new();

    public Guid? ActiveCompanyId { get; set; }
    public string? ActiveCompanyName { get; set; }
    public Guid? ActiveStoreId { get; set; }

    public IReadOnlyList<StoreClerkAccountDraft> CreatedClerks => _createdClerks;

    public void TrackCreatedClerk(StoreClerkAccountDraft draft)
    {
        _createdClerks.Insert(0, draft);
        if (_createdClerks.Count > 20)
        {
            _createdClerks.RemoveRange(20, _createdClerks.Count - 20);
        }
    }
}
