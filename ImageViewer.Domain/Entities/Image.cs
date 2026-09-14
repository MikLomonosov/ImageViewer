using ImageViewer.Domain.Common;
using ImageViewer.Domain.ValueObjects;

namespace ImageViewer.Domain.Entities;

public sealed class Image : Entity<Guid>
{
    public string Name { get; private set; }
    public string Path { get; private set; }
    public DateTimeOffset CreatedDateUtc { get; private set; }
    public FileSize Size { get; private set; }
    public ImageDimensions? Dimensions { get; private set; }
    public bool HasThumbnail => _thumbnail is not null;
    public bool HasOriginalData => _originalData is not null;
    public ImageBinaryData? Thumbnail => _thumbnail;
    public ImageBinaryData? OriginalData => _originalData;
    

    private ImageBinaryData? _thumbnail;
    private ImageBinaryData? _originalData;

    #region constructors

    private Image(Guid id,
                    string name,
                    string path,
                    DateTimeOffset createdDateUtc,
                    FileSize size,
                    ImageDimensions? dimensions) :  base(id)
    {
        Name = name;
        Path = path;
        CreatedDateUtc = createdDateUtc;
        Size = size;
        Dimensions = dimensions;
    }
    
    #endregion

    public static Image Create(string name,
                                string path,
                                DateTimeOffset createdDateUtc,
                                FileSize size,
                                ImageDimensions? dimensions)
    {
        if (string.IsNullOrEmpty(name)) 
            throw new ArgumentException("Имя файла не может быть пустым.",  nameof(name));
        
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(path));
        
        if (createdDateUtc.Offset != TimeSpan.Zero)
            throw new ArgumentException("Дата должна быть в UTC (Offset = 0).", nameof(createdDateUtc));
        
        return new Image(Guid.NewGuid(), name, path, createdDateUtc, size, dimensions);
    }

    public static Image Restore(Guid id,
                                string name,
                                string path,
                                DateTimeOffset createdDateUtc,
                                FileSize size,
                                ImageDimensions? dimensions,
                                ImageBinaryData? thumbnail,
                                ImageBinaryData? originalData)
    {
        var image = new Image(id, name, path, createdDateUtc, size, dimensions);
        image._thumbnail =  thumbnail;
        image._originalData = originalData;
        
        return image;
    }

    public void AttachThumbnail(ImageBinaryData thumbnail)
    {
        _thumbnail = thumbnail ?? throw new ArgumentNullException(nameof(thumbnail));
    }

    public void AttachOriginalData(ImageBinaryData originalData)
    {
        _originalData = originalData ?? throw new ArgumentNullException(nameof(originalData));
    }
}