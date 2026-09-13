using ImageViewer.Application.DTO;
using ImageViewer.Domain.Entities;
using ImageViewer.Domain.ValueObjects;

namespace ImageViewer.Application.Mapping;

public class ImageMapper
{
    public static ImageDto ToDto(Image image)
    {
        return new ImageDto
        {
            Id = image.Id,
            Name = image.Name,
            Path = image.Path,
            Size = image.Size.Bytes,
            CreatedDateUtc = image.CreatedDateUtc,
            Width = image.Dimensions?.Width,
            Height = image.Dimensions?.Height,
            Thumbnail = image.Thumbnail?.ToArray(),
            OriginalData = image.OriginalData?.ToArray()
        };
    }

    public static Image ToDomain(ImageDto dto)
    {
        var dimensions = dto is { Width: not null, Height: not null }
            ? ImageDimensions.Create(dto.Width.Value, dto.Height.Value)
            : null;
        
        var thumbnail = dto.Thumbnail is not null ? ImageBinaryData.CreateFromBytes(dto.Thumbnail) : null;
        var originalData = dto.OriginalData is not null ? ImageBinaryData.CreateFromBytes(dto.OriginalData) : null;
        
        return Image.Restore(dto.Id,
            dto.Name,
            dto.Path,
            dto.CreatedDateUtc,
            FileSize.FromBytes(dto.Size),
            dimensions,
            thumbnail,
            originalData);
    }
}