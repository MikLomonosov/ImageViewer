using System.IO;
using System.Text;
using ImageViewer.Application.DTO;
using ImageViewer.Infrastructure.Serialization;
using Xunit;

namespace ImageViewer.Tests.Infrastructure;

public class BinaryImageSerializerTests
{
    private readonly BinaryImageSerializer _serializer = new();

    private static ImageCollectionDto BuildCollection()
    {
        var dto = new ImageCollectionDto
        {
            Id = Guid.NewGuid(),
            CreatedDateUtc = DateTimeOffset.FromUnixTimeMilliseconds(1_750_000_000_000)
        };

        dto.Images.Add(new ImageDto
        {
            Id = Guid.NewGuid(),
            Name = "cat.png",
            Path = @"C:\images\cat.png",
            Size = 123_456,
            CreatedDateUtc = DateTimeOffset.FromUnixTimeMilliseconds(1_750_000_000_001),
            Width = 640,
            Height = 480,
            Thumbnail = new ReadOnlyMemory<byte>(new byte[] { 1, 2, 3 }),
            OriginalData = new ReadOnlyMemory<byte>(new byte[] { 4, 5 })
        });

        dto.Images.Add(new ImageDto
        {
            Id = Guid.NewGuid(),
            Name = "unknown.bin",
            Path = @"C:\images\unknown.bin",
            Size = 1,
            CreatedDateUtc = DateTimeOffset.FromUnixTimeMilliseconds(1_750_000_000_002)
            // no dimensions, no binary data
        });

        return dto;
    }

    [Fact]
    public async Task SerializeDeserialize_RoundTrips_AllFields()
    {
        var original = BuildCollection();

        using var stream = new MemoryStream();
        await _serializer.SerializeAsync(original, stream);
        stream.Position = 0;

        var restored = await _serializer.DeserializeAsync(stream);

        Assert.Equal(original.Id, restored.Id);
        Assert.Equal(original.CreatedDateUtc, restored.CreatedDateUtc);
        Assert.Equal(2, restored.Images.Count);

        var first = restored.Images[0];
        Assert.Equal(original.Images[0].Id, first.Id);
        Assert.Equal("cat.png", first.Name);
        Assert.Equal(@"C:\images\cat.png", first.Path);
        Assert.Equal(123_456, first.Size);
        Assert.Equal(original.Images[0].CreatedDateUtc, first.CreatedDateUtc);
        Assert.Equal(640, first.Width);
        Assert.Equal(480, first.Height);
        Assert.True(new byte[] { 1, 2, 3 }.AsSpan().SequenceEqual(first.Thumbnail!.Value.Span));
        Assert.True(new byte[] { 4, 5 }.AsSpan().SequenceEqual(first.OriginalData!.Value.Span));

        var second = restored.Images[1];
        Assert.Equal("unknown.bin", second.Name);
        Assert.Null(second.Width);
        Assert.Null(second.Height);
        Assert.Null(second.Thumbnail);
        Assert.Null(second.OriginalData);
    }

    [Fact]
    public async Task SerializeDeserialize_EmptyCollection_PreservesIdentity()
    {
        var original = new ImageCollectionDto
        {
            Id = Guid.NewGuid(),
            CreatedDateUtc = DateTimeOffset.FromUnixTimeMilliseconds(42)
        };

        using var stream = new MemoryStream();
        await _serializer.SerializeAsync(original, stream);
        stream.Position = 0;

        var restored = await _serializer.DeserializeAsync(stream);

        Assert.Equal(original.Id, restored.Id);
        Assert.Equal(42L, restored.CreatedDateUtc.ToUnixTimeMilliseconds());
        Assert.Empty(restored.Images);
    }

    [Fact]
    public async Task Deserialize_WithUnsupportedVersion_Throws()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(int.MaxValue);
        }
        stream.Position = 0;

        await Assert.ThrowsAsync<InvalidOperationException>(() => _serializer.DeserializeAsync(stream));
    }
}
