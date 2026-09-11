namespace ImageViewer.Domain.ValueObjects;

public sealed class ImageBinaryData
{
    public int LengthBytes => _data.Length;
    private readonly byte[] _data;
    
    #region constructors

    private ImageBinaryData(byte[] data)
    {
        _data = data;
    }
    
    #endregion

    public static ImageBinaryData CreateFromBytes(byte[] data)
    {
        if (data is null || data.Length == 0)
            throw new ArgumentNullException(nameof(data), "Данные изображения не могут быть путсыми.");

        return new ImageBinaryData(data);
    }
    
    public byte[] ToArray() => (byte[])_data.Clone();

    public bool Equals(ImageBinaryData? other)
    {
        return other is not null && _data.AsSpan().SequenceEqual(other._data);
    }

    public override bool Equals(object? other)
    {
        return Equals(other as ImageBinaryData);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.AddBytes(_data);
        
        return hash.ToHashCode();
    }
}