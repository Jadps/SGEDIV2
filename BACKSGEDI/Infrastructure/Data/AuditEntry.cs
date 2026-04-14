using Microsoft.EntityFrameworkCore.ChangeTracking;
using BACKSGEDI.Domain.Entities;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace BACKSGEDI.Infrastructure.Data;

public class AuditEntry
{
    public AuditEntry(EntityEntry entry)
    {
        Entry = entry;
    }

    public EntityEntry Entry { get; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? TraceId { get; set; }
    
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, object> KeyValues { get; } = new();
    public Dictionary<string, object> OldValues { get; } = new();
    public Dictionary<string, object> NewValues { get; } = new();
    public List<PropertyEntry> TemporaryProperties { get; } = new();
    public string AuditType { get; set; } = string.Empty;
    public List<string> ChangedColumns { get; } = new();

    public bool HasTemporaryProperties => TemporaryProperties.Any();

    public AuditLog ToAudit()
    {
        var audit = new AuditLog
        {
            UserId = UserId,
            IpAddress = IpAddress,
            UserAgent = UserAgent,
            TraceId = TraceId,
            AuditType = AuditType,
            TableName = TableName,
            CreatedAt = DateTime.UtcNow,
            PrimaryKey = JsonSerializer.Serialize(KeyValues),
            OldValues = OldValues.Any() ? JsonSerializer.Serialize(OldValues) : null,
            NewValues = NewValues.Any() ? JsonSerializer.Serialize(NewValues) : null,
            ChangedColumns = ChangedColumns.Any() ? JsonSerializer.Serialize(ChangedColumns) : null
        };
        return audit;
    }
}
