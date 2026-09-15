using ImageViewer.Application.DTO;
using ImageViewer.Domain.Aggregates;
using ImageViewer.Domain.Entities;
using ImageViewer.Domain.ValueObjects;

namespace ImageViewer.Application.Mapping;

public static class ImageMapper
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
            Thumbnail = image.Thumbnail?.AsMemory(),
            OriginalData = image.OriginalData?.AsMemory()
        };
    }

    public static Image ToDomain(ImageDto dto)
    {
        var dimensions = dto is { Width: not null, Height: not null }
            ? ImageDimensions.Create(dto.Width.Value, dto.Height.Value)
            : null;
        
        var thumbnail = dto.Thumbnail is {  } t ? ImageBinaryData.CreateFromBytes(t.ToArray()) 
            : null;
        var originalData = dto.OriginalData is { } o ? ImageBinaryData.CreateFromBytes(o.ToArray()) 
            : null;
        
        return Image.Restore(dto.Id,
                            dto.Name,
                            dto.Path,
                            dto.CreatedDateUtc,
                            FileSize.FromBytes(dto.Size),
                            dimensions,
                            thumbnail,
                            originalData);
    }

    public static ImageCollectionDto ToDto(ImageCollection imageCollection)
    {
        return new ImageCollectionDto
        {
            Id = imageCollection.Id,
            CreatedDateUtc = imageCollection.CratedAtUtc,
            Images = imageCollection.Images.Select(ToDto).ToList()
        };
    }

    public static ImageCollection ToDomain(ImageCollectionDto dto) => 
        ImageCollection.Restore(dto.Id, dto.CreatedDateUtc, dto.Images.Select(ToDomain));
}