namespace ImageViewer.Domain.ValueObjects;

public sealed record ImageDimensions {
    
    public int Width { get; }
    public int Height { get; }
    
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

    public override string ToString()
    {
        return $"{Width}x{Height}";
    }
    
}