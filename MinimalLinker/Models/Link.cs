using System.ComponentModel.DataAnnotations;

namespace MinimalLinker;

public class Link
{
    public required int id { get; set; }
    public string fullUrl { get; set; }
}