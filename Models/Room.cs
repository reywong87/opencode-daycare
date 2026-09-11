using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDaycare.Models;

[Table("rooms")]
public sealed class Room : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("daycare_id")]
    public Guid DaycareId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}
