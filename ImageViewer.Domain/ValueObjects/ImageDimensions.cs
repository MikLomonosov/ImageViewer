namespace ImageViewer.Domain.ValueObjects;

public sealed class ImageDimensions : IEquatable<ImageDimensions>
{
    public int Width { get; }
    public int Height { get; }
    public double AspectRatio => (double)Width / Height;
    
    
    #region constructors
    
    private ImageDimensions(int width, int height)
    {
        Width = width;
        Height = height;
    }

    #endregion
    
    public static ImageDimensions Create(int width, int height)
    {
        if (width <= 0 ||  height <= 0)
            throw new ArgumentException("Ширина и высота изображения должны быть больше 0.");

        return new ImageDimensions(width, height);
    }

    public bool Equals(ImageDimensions? other)
    {
        return other is not null && Width == other.Width && Height == other.Height;
    }

    public override bool Equals(object? other)
    {
        return Equals(other as ImageDimensions);
    }

    public override int GetHashCode()
    {
        return  HashCode.Combine(Width, Height);
    }

    public override string ToString()
    {
        return $"{Width}x{Height}";
    }
    
}