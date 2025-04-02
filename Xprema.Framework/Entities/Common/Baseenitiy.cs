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

    public void AddHistoryRecord(string changedBy, string changeType, string? changeDetails = null)
    {
        HistoryRecords.Add(new EntityHistoryRecord
        {
            ChangeDate = DateTime.Now,
            ChangedBy = changedBy,
            ChangeType = changeType,
            ChangeDetails = changeDetails
        });
    }
}
