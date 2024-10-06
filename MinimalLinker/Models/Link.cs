using System.ComponentModel.DataAnnotations;

namespace MinimalLinker;

public class Link
{
    [MaxLength(10)]
    public required string id { get; set; }
    public string fullUrl { get; set; }
}