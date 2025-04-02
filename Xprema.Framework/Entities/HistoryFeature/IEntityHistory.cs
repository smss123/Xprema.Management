namespace Xprema.Framework.Entities.HistoryFeature;

public interface IEntityHistory
{
    List<EntityHistoryRecord> HistoryRecords { get; set; }

}


public class EntityHistoryRecord
{
    public int Id { get; set; } // Primary key
    public DateTime ChangeDate { get; set; }
    public string ChangedBy { get; set; } = null!;
    public string ChangeType { get; set; } = null!;
    public string? ChangeDetails { get; set; }
}
