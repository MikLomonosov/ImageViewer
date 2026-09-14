using ImageViewer.Domain.Aggregates;

namespace ImageViewer.Domain.Repositories;

public interface IImageCollectionRepository
{
    Task<IReadOnlyList<string>> SaveAsync(ImageCollection imageCollection,
                                            string filePath, 
                                            CancellationToken cancellationToken = default);
    Task<ImageCollection> LoadAsync(string filePath, CancellationToken cancellationToken = default);
}