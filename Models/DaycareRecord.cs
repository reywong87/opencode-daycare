using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace OpenDaycare.Models;

[Table("daycares")]
public sealed class DaycareRecord : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;
}
