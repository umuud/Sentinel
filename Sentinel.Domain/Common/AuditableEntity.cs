namespace Sentinel.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = default!;

    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedBy { get; set; }
}