using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDaycare.Models;

[Table("post_photos")]
public sealed class PostPhoto : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("post_id")]
    public Guid PostId { get; set; }

    [Column("storage_path")]
    public string StoragePath { get; set; } = string.Empty;

    [Column("position")]
    public short Position { get; set; }

    [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
    public DateTimeOffset CreatedAt { get; set; }
}
