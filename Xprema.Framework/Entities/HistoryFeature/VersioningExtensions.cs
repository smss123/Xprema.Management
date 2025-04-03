using System.Text;
using System.Text.Json;
using Xprema.Framework.Entities.Common;

namespace Xprema.Framework.Entities.HistoryFeature;

public static class VersioningExtensions
{
    /// <summary>
    /// Gets a list of all available versions for an entity
    /// </summary>
    public static IEnumerable<EntityVersionInfo> GetAvailableVersions<TKey>(this BaseEntity<TKey> entity)
    {
        return entity.HistoryRecords
            .OrderByDescending(r => r.VersionNumber)
            .Select(r => new EntityVersionInfo
            {
                VersionNumber = r.VersionNumber,
                ChangeDate = r.ChangeDate,
                ChangedBy = r.ChangedBy,
                ChangeType = r.ChangeType,
                ChangedProperties = r.PropertyChanges.Select(p => p.PropertyName).ToList()
            });
    }
    
    /// <summary>
    /// Compares two versions of an entity and returns a diff
    /// </summary>
    public static EntityVersionDiff CompareVersions<TKey>(this BaseEntity<TKey> entity, int versionA, int versionB)
    {
        var recordA = entity.HistoryRecords.FirstOrDefault(r => r.VersionNumber == versionA);
        var recordB = entity.HistoryRecords.FirstOrDefault(r => r.VersionNumber == versionB);
        
        if (recordA == null || recordB == null)
            throw new ArgumentException($"One or both versions ({versionA}, {versionB}) not found");
            
        var diff = new EntityVersionDiff
        {
            VersionA = versionA,
            VersionB = versionB,
            DateA = recordA.ChangeDate,
            DateB = recordB.ChangeDate,
            ChangedBy = recordB.ChangedBy,
            ChangeType = recordB.ChangeType
        };
        
        // Get all properties that changed in version B
        foreach (var change in recordB.PropertyChanges)
        {
            diff.PropertyDiffs.Add(new PropertyVersionDiff
            {
                PropertyName = change.PropertyName,
                OldValue = change.OldValue,
                NewValue = change.NewValue
            });
        }
        
        return diff;
    }
    
    /// <summary>
    /// Generates a human-readable change summary for a specific version
    /// </summary>
    public static string GenerateChangeSummary<TKey>(this BaseEntity<TKey> entity, int version)
    {
        var record = entity.HistoryRecords.FirstOrDefault(r => r.VersionNumber == version);
        if (record == null)
            throw new ArgumentException($"Version {version} not found");
            
        var sb = new StringBuilder();
        sb.AppendLine($"Version {version} - {record.ChangeType} on {record.ChangeDate} by {record.ChangedBy}");
        sb.AppendLine("Changes:");
        
        foreach (var change in record.PropertyChanges)
        {
            sb.AppendLine($"  - {change.PropertyName}: {change.OldValue} → {change.NewValue}");
        }
        
        return sb.ToString();
    }
}

public class EntityVersionInfo
{
    public int VersionNumber { get; set; }
    public DateTime ChangeDate { get; set; }
    public string ChangedBy { get; set; } = null!;
    public string ChangeType { get; set; } = null!;
    public List<string> ChangedProperties { get; set; } = new();
}

public class EntityVersionDiff
{
    public int VersionA { get; set; }
    public int VersionB { get; set; }
    public DateTime DateA { get; set; }
    public DateTime DateB { get; set; }
    public string ChangedBy { get; set; } = null!;
    public string ChangeType { get; set; } = null!;
    public List<PropertyVersionDiff> PropertyDiffs { get; set; } = new();
}

public class PropertyVersionDiff
{
    public string PropertyName { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    
    public T? GetTypedOldValue<T>() => string.IsNullOrEmpty(OldValue) ? default : JsonSerializer.Deserialize<T>(OldValue);
    public T? GetTypedNewValue<T>() => string.IsNullOrEmpty(NewValue) ? default : JsonSerializer.Deserialize<T>(NewValue);
} 