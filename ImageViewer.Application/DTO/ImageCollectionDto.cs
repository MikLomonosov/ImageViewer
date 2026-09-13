namespace ImageViewer.Application.DTO;

public sealed class ImageCollectionDto
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedDateUtc { get; set; }
    public List<ImageDto> Images { get; set; } = new();
}