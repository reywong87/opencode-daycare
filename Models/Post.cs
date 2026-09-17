using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDaycare.Models;

[Table("posts")]
public sealed class Post : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("daycare_id")]
    public Guid DaycareId { get; set; }

    [Column("author_id")]
    public Guid AuthorId { get; set; }

    [Column("type")]
    public string Type { get; set; } = string.Empty;

    [Column("body")]
    public string Body { get; set; } = string.Empty;

    [Column("published_at")]
    public DateTimeOffset PublishedAt { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; }
}
