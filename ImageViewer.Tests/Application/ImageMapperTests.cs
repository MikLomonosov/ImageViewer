using ImageViewer.Application.DTO;
using ImageViewer.Application.Mapping;
using ImageViewer.Domain.Aggregates;
using ImageViewer.Domain.Entities;
using ImageViewer.Domain.ValueObjects;
using Xunit;

namespace ImageViewer.Tests.Application;

public class ImageMapperTests
{
    private static readonly DateTimeOffset Created = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ToDto_MapsAllFields()
    {
        var thumbnail = ImageBinaryData.CreateFromBytes(new byte[] { 1, 2 });
        var image = Image.Restore(Guid.NewGuid(), "cat.png", @"C:\cat.png", Created,
            FileSize.FromBytes(42), ImageDimensions.Create(10, 20), thumbnail, null);

        var dto = ImageMapper.ToDto(image);

        Assert.Equal(image.Id, dto.Id);
        Assert.Equal("cat.png", dto.Name);
        Assert.Equal(@"C:\cat.png", dto.Path);
        Assert.Equal(42, dto.Size);
        Assert.Equal(Created, dto.CreatedDateUtc);
        Assert.Equal(10, dto.Width);
        Assert.Equal(20, dto.Height);
        Assert.True(new byte[] { 1, 2 }.AsSpan().SequenceEqual(dto.Thumbnail!.Value.Span));
        Assert.Null(dto.OriginalData);
    }

    [Fact]
    public void ToDomain_IsInverseOfToDto()
    {
        var thumbnail = ImageBinaryData.CreateFromBytes(new byte[] { 1, 2 });
        var original = ImageBinaryData.CreateFromBytes(new byte[] { 3, 4, 5 });
        var image = Image.Restore(Guid.NewGuid(), "cat.png", @"C:\cat.png", Created,
            FileSize.FromBytes(42), ImageDimensions.Create(10, 20), thumbnail, original);

        var restored = ImageMapper.ToDomain(ImageMapper.ToDto(image));

        Assert.Equal(image.Id, restored.Id);
        Assert.Equal(image.Name, restored.Name);
        Assert.Equal(image.Path, restored.Path);
        Assert.Equal(image.Size, restored.Size);
        Assert.Equal(image.CreatedDateUtc, restored.CreatedDateUtc);
        Assert.Equal(image.Dimensions, restored.Dimensions);
        Assert.True(image.Thumbnail!.AsMemory().Span.SequenceEqual(restored.Thumbnail!.AsMemory().Span));
        Assert.True(image.OriginalData!.AsMemory().Span.SequenceEqual(restored.OriginalData!.AsMemory().Span));
    }

    [Fact]
    public void ToDomain_WithoutDimensions_PreservesNull()
    {
        var dto = new ImageDto
        {
            Id = Guid.NewGuid(),
            Name = "a.png",
            Path = @"C:\a.png",
            Size = 1,
            CreatedDateUtc = Created
        };

        var restored = ImageMapper.ToDomain(dto);

        Assert.Null(restored.Dimensions);
        Assert.Null(restored.Thumbnail);
        Assert.Null(restored.OriginalData);
    }

    [Fact]
    public void CollectionMapping_IsInverseOfItself()
    {
        var id = Guid.NewGuid();
        var image = Image.Create("a.png", @"C:\a.png", Created, FileSize.FromBytes(1), null);
        var collection = ImageCollection.Restore(id, Created, new[] { image });

        var roundTripped = ImageMapper.ToDomain(ImageMapper.ToDto(collection));

        Assert.Equal(id, roundTripped.Id);
        Assert.Equal(Created, roundTripped.CratedAtUtc);
        var restoredImage = Assert.Single(roundTripped.Images);
        Assert.Equal(image.Id, restoredImage.Id);
        Assert.Equal(@"C:\a.png", restoredImage.Path);
    }
}
