namespace Xprema.Framework.Entities.HistoryFeature;

public interface IEntityHistory
{
    List<EntityHistoryRecord> HistoryRecords { get; set; }
    
    /// <summary>
    /// Adds a history record with details about which properties changed
    /// </summary>
    void AddVersionRecord(string changedBy, string changeType, Dictionary<string, (object? OldValue, object? NewValue)>? propertyChanges = null);
    
    /// <summary>
    /// Gets a specific version of the entity at a given point in time
    /// </summary>
    T GetVersion<T>(DateTime pointInTime) where T : class, IEntityHistory;
}


public class EntityHistoryRecord
{
    public int Id { get; set; } // Primary key
    public DateTime ChangeDate { get; set; }
    public string ChangedBy { get; set; } = null!;
    public string ChangeType { get; set; } = null!;
    public string? ChangeDetails { get; set; }
    
    /// <summary>
    /// Sequential version number, incremented with each change
    /// </summary>
    public int VersionNumber { get; set; }
    
    /// <summary>
    /// Collection of property changes associated with this history record
    /// </summary>
    public List<EntityPropertyChangeRecord> PropertyChanges { get; set; } = new();
}
