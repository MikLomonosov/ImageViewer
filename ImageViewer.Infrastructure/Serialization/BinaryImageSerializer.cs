using System.IO;
using System.Text;
using ImageViewer.Application.DTO;
using ImageViewer.Application.Interfaces;

namespace ImageViewer.Infrastructure.Serialization;

public class BinaryImageSerializer : IImageSerializer
{
    private const int FormatVersion = 1;
    public string FileExtension => ".imgcollection";

    public async Task SerializeAsync(ImageCollectionDto imageCollectionDto, 
                                    Stream output,
                                    CancellationToken cancellationToken = default)
    {
        await using var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen:true);
        
        writer.Write(FormatVersion);
        writer.Write(imageCollectionDto.Id.ToByteArray());
        writer.Write(imageCollectionDto.CreatedDateUtc.ToUnixTimeMilliseconds());
        writer.Write(imageCollectionDto.Images.Count);

        foreach (var image in imageCollectionDto.Images)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            writer.Write(image.Id.ToByteArray());
            writer.Write(image.Name);
            writer.Write(image.Path);
            writer.Write(image.Size);
            writer.Write(image.CreatedDateUtc.ToUnixTimeMilliseconds());
            
            writer.Write(image.Width.HasValue);

            if (image.Width.HasValue)
            {
                writer.Write(image.Width.Value);
                writer.Write(image.Height!.Value);
            }
            
            WriteNullableBytes(writer, image.Thumbnail);
            WriteNullableBytes(writer, image.OriginalData);
        }
        
        writer.Flush();
    }

    public Task<ImageCollectionDto> DeserializeAsync(Stream input, CancellationToken cancellationToken = default)
    {
        using var reader = new BinaryReader(input, Encoding.UTF8, leaveOpen:true);
        
        var version = reader.ReadInt32();

        if (version != FormatVersion)
            throw new InvalidOperationException($"Неподдерживаемая версия формата файла: {version}.");

        var imageCollection = new ImageCollectionDto()
        {
            Id = new Guid(reader.ReadBytes(16)),
            CreatedDateUtc = DateTimeOffset.FromUnixTimeMilliseconds(reader.ReadInt64()),
        };

        var itemsCount = reader.ReadInt32();

        for (int i = 0; i < itemsCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var image = new ImageDto
            {
                Id = new Guid(reader.ReadBytes(16)),
                Name = reader.ReadString(),
                Path = reader.ReadString(),
                Size = reader.ReadInt64(),
                CreatedDateUtc = DateTimeOffset.FromUnixTimeMilliseconds(reader.ReadInt64())
            };

            if (reader.ReadBoolean())
            {
                image.Width = reader.ReadInt32();
                image.Height = reader.ReadInt32();
            }
            
            image.Thumbnail = ReadNullableBytes(reader);
            image.OriginalData = ReadNullableBytes(reader);
            
            imageCollection.Images.Add(image);
        }
        
        return Task.FromResult(imageCollection);
    }

    private static void WriteNullableBytes(BinaryWriter writer, ReadOnlyMemory<byte>? data)
    {
        writer.Write(data is not null);

        if (data is null)
            return;
        
        writer.Write(data.Value.Length);
        writer.Write(data.Value.Span);
    }

    private static ReadOnlyMemory<byte>? ReadNullableBytes(BinaryReader reader)
    {
        if (!reader.ReadBoolean())
            return null;

        var length = reader.ReadInt32();
        
        return reader.ReadBytes(length);
    }
}