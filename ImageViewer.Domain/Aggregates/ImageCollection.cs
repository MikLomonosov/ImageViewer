using ImageViewer.Domain.Entities;

namespace ImageViewer.Domain.Aggregates;

public sealed class ImageCollection
{
    public Guid Id { get; }
    public DateTimeOffset CratedAtUtc { get; }

    private readonly List<Image> _images = new();
    
    public IReadOnlyList<Image> Images => _images.AsReadOnly();
    public int Count => _images.Count;
    
    #region constructors

    private ImageCollection(Guid id, DateTimeOffset cratedAtUtc)
    {
        Id = id;
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

        if (_images.Any(existing => existing.Path == image.Path))
            throw new InvalidOperationException($"Изображение с адресом \"{image.Path}\" уже добавлено");
        
        _images.Add(image);
    }

    public IReadOnlyCollection<Image> AddRange(IEnumerable<Image> images)
    {
        var added = new List<Image>();

        foreach (var image in images)
        {
            if (_images.Any(existing => existing.Path == image.Path))
                continue;
            
            _images.Add(image);
            added.Add(image);
        }

        return added;
    }

    // for the future
    // also is not used now
    public bool RemoveItem(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);

        if (image is null)
            return false;
        
        _images.Remove(image);
        
        return true;
    }

    public void Clear()
    {
        _images.Clear();
    }

    // for the future
    // also is not used now
    public Image? FindById(Guid id)
    {
        return _images.FirstOrDefault(i => i.Id == id);
    }

    // it was added for clearing list after saving/serializing image collection
    // but now it is not used, 'cause I think it is unnecessary function
    public void ReleaseAllOriginalData()
    {
        foreach (var image in _images)
            image.ReleaseOriginalData();
    }

    public override bool Equals(object? other)
    {
        return other is ImageCollection otherImageCollection && Id.Equals(otherImageCollection.Id);
    }
    
    public override int GetHashCode() => Id.GetHashCode();
}