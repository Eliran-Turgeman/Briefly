namespace Briefly;

public class PublishedMessage
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public string Media { get; set; } = string.Empty;
    public string MediaIdentifier { get; set; } = string.Empty;
}
