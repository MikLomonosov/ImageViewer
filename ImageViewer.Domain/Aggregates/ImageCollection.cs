using ImageViewer.Domain.Common;
using ImageViewer.Domain.Entities;

namespace ImageViewer.Domain.Aggregates;

public sealed class ImageCollection : Entity<Guid>
{
    public DateTimeOffset CratedAtUtc { get; }

    private readonly List<Image> _images = new();
    
    public IReadOnlyList<Image> Images => _images.AsReadOnly();
    public int Count => _images.Count;
    private readonly HashSet<string> _sourcePaths = new (StringComparer.OrdinalIgnoreCase);
    
    #region constructors

    private ImageCollection(Guid id, DateTimeOffset cratedAtUtc) : base(id)
    {
        CratedAtUtc = cratedAtUtc;
    }
    
    #endregion

    public static ImageCollection Create()
    {
        return new ImageCollection(Guid.NewGuid(), DateTimeOffset.UtcNow);
    }

    public static ImageCollection Restore(Guid id, DateTimeOffset cratedAtUtc, IEnumerable<Image> images)
    {
        var collection = new ImageCollection(id, cratedAtUtc);
        
        foreach (var image in images)
            collection.Add(image);
        
        return collection;
    }
    
    public void Add(Image image)
    {
        if (image is null)
            throw new ArgumentNullException(nameof(image), "Изображение не может быть пустым.");

        if (!_sourcePaths.Add(image.Path))
            throw new InvalidOperationException($"Изображение с адресом \"{image.Path}\" уже добавлено");
        
        _images.Add(image);
    }

    public IReadOnlyCollection<Image> AddRange(IEnumerable<Image> images)
    {
        var added = new List<Image>();

        foreach (var image in images)
        {
            if (!_sourcePaths.Add(image.Path))
                continue;
            
            _images.Add(image);
            added.Add(image);
        }

        return added;
    }
    
    public void Clear()
    {
        _images.Clear();
        _sourcePaths.Clear();
    }
}