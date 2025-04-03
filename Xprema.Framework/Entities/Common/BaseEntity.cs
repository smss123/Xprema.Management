using System.Linq;
using System.Reflection;
using Xprema.Framework.Entities.HistoryFeature;

namespace Xprema.Framework.Entities.Common;

public class BaseEntity<TKey> : IFullAudit,IEntityHistory
{
    public required TKey Id { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedDate { get; set; }
    public bool IsDeleted { get; set; } = false;

    public List<EntityHistoryRecord> HistoryRecords { get; set; } = new();

    /// <summary>
    /// Basic method to add a history record without property change tracking
    /// </summary>
    public void AddHistoryRecord(string changedBy, string changeType, string? changeDetails = null)
    {
        var nextVersion = HistoryRecords.Any() ? HistoryRecords.Max(r => r.VersionNumber) + 1 : 1;
        
        HistoryRecords.Add(new EntityHistoryRecord
        {
            ChangeDate = DateTime.Now,
            ChangedBy = changedBy,
            ChangeType = changeType,
            ChangeDetails = changeDetails,
            VersionNumber = nextVersion
        });
    }
    
    /// <summary>
    /// Adds a history record with detailed property change tracking
    /// </summary>
    public void AddVersionRecord(string changedBy, string changeType, Dictionary<string, (object? OldValue, object? NewValue)>? propertyChanges = null)
    {
        var nextVersion = HistoryRecords.Any() ? HistoryRecords.Max(r => r.VersionNumber) + 1 : 1;
        
        var record = new EntityHistoryRecord
        {
            ChangeDate = DateTime.Now,
            ChangedBy = changedBy,
            ChangeType = changeType,
            VersionNumber = nextVersion
        };
        
        if (propertyChanges != null)
        {
            foreach (var change in propertyChanges)
            {
                record.PropertyChanges.Add(new EntityPropertyChangeRecord
                {
                    PropertyName = change.Key,
                    OldValue = PropertyChangeExtensions.SerializeValue(change.Value.OldValue),
                    NewValue = PropertyChangeExtensions.SerializeValue(change.Value.NewValue)
                });
            }
            
            // Add summary of changes to ChangeDetails
            record.ChangeDetails = $"Changed properties: {string.Join(", ", propertyChanges.Keys)}";
        }
        
        HistoryRecords.Add(record);
    }
    
    /// <summary>
    /// Gets a specific version of the entity at a given point in time
    /// </summary>
    public T GetVersion<T>(DateTime pointInTime) where T : class, IEntityHistory
    {
        if (!(this is T currentEntity))
            throw new InvalidOperationException("Cannot cast the current entity to the requested type");
            
        // Find the latest record before or at the specified time
        var relevantRecords = HistoryRecords
            .Where(r => r.ChangeDate <= pointInTime)
            .OrderBy(r => r.ChangeDate)
            .ToList();
            
        if (!relevantRecords.Any())
            throw new InvalidOperationException($"No version exists at or before {pointInTime}");
            
        // Create a deep copy of the current entity
        T historicalEntity = CreateDeepCopy<T>();
        
        // Apply all changes in reverse order to reconstruct the entity state
        foreach (var record in relevantRecords)
        {
            // Skip if this is a creation record (first version)
            if (record.ChangeType == "Created")
                continue;
                
            // For each property change, apply the old value
            foreach (var propertyChange in record.PropertyChanges)
            {
                var property = typeof(T).GetProperty(propertyChange.PropertyName);
                if (property != null && property.CanWrite)
                {
                    var oldValue = GetTypedValue(property, propertyChange.OldValue);
                    property.SetValue(historicalEntity, oldValue);
                }
            }
        }
        
        return historicalEntity;
    }
    
    private T CreateDeepCopy<T>() where T : class
    {
        // Create a new instance of T
        var copy = Activator.CreateInstance<T>();
        
        // Copy all property values
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (property.CanWrite && property.CanRead)
            {
                var value = property.GetValue(this);
                property.SetValue(copy, value);
            }
        }
        
        return copy;
    }
    
    private object? GetTypedValue(PropertyInfo property, string? serializedValue)
    {
        if (string.IsNullOrEmpty(serializedValue))
            return null;
            
        var methodInfo = typeof(PropertyChangeExtensions)
            .GetMethod(nameof(PropertyChangeExtensions.DeserializeValue))
            ?.MakeGenericMethod(property.PropertyType);
            
        if (methodInfo == null)
            throw new InvalidOperationException($"Failed to get DeserializeValue method for type {property.PropertyType}");
            
        return methodInfo.Invoke(null, new object[] { serializedValue });
    }
}
