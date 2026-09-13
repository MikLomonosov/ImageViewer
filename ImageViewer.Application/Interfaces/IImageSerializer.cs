using System.IO;
using ImageViewer.Application.DTO;

namespace ImageViewer.Application.Interfaces;

public interface IImageSerializer
{
    string FileExtension { get; }
    
    Task SerializeAsync(ImageCollectionDto imageCollectionDto,
                        Stream output,
                        CancellationToken cancellationToken = default);
    
    Task<ImageCollectionDto> DeserializeAsync(Stream input,
                                                CancellationToken cancellationToken = default);
}