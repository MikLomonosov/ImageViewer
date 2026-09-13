namespace ImageViewer.Application.DTO;

public sealed class ImageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTimeOffset CreatedDateUtc { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public byte[]? Thumbnail { get; set; }
    public byte[]? OriginalData { get; set; }
}