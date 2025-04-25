namespace Truckero.Core.Entities;

public class SystemSetting
{
    public string Key { get; set; } = null!;  // ← Acts as the primary key
    public string Value { get; set; } = null!;
    public string? Description { get; set; }
}


