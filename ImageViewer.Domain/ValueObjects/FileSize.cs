namespace ImageViewer.Domain.ValueObjects;

public record FileSize
{
    public long Bytes { get;  }
    public double Kilobytes => Bytes / 1024.0;
    public double Megabytes => Bytes / (1024.0 * 1024.0);

    
    #region constructors
    
    private FileSize(long bytes)
    {
        Bytes = bytes;
    }
    
    #endregion

    public static FileSize FromBytes(long bytes)
    {
        return bytes < 0 
            ? throw new ArgumentOutOfRangeException(nameof(bytes), "Размер файла не можем быть отрицательным!") 
            : new FileSize(bytes);
    }
    
    public override string ToString()
    {
        return Bytes switch
        {
            < 1024 => $"{Bytes} Б",
            < 1024 * 1024 => $"{Kilobytes:F1} КБ",
            _ => $"{Megabytes:F1} МБ"
        };
    }
    
}