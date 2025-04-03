using System.Text.Json;

namespace Xprema.Framework.Entities.HistoryFeature;

public class EntityPropertyChangeRecord
{
    public int Id { get; set; }
    public int EntityHistoryRecordId { get; set; }
    public EntityHistoryRecord EntityHistoryRecord { get; set; } = null!;
    public string PropertyName { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

public static class PropertyChangeExtensions
{
    public static string? SerializeValue(object? value)
    {
        if (value == null)
            return null;
            
        return JsonSerializer.Serialize(value);
    }
    
    public static T? DeserializeValue<T>(string? serializedValue)
    {
        if (string.IsNullOrEmpty(serializedValue))
            return default;
            
        return JsonSerializer.Deserialize<T>(serializedValue);
    }
} 