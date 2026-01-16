using System.ComponentModel.DataAnnotations;

namespace CaraDog.Db.Entities;

public sealed class Tag
{
    public Guid Id { get; set; }

    [MaxLength(80)]
    public string Name { get; set; } = string.Empty;

    public ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();
}
