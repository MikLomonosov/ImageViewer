namespace ImageViewer.Domain.ValueObjects;

public sealed class FileSize
{
    public long Bytes { get;  }
    public double Kilobytes => Bytes / 1024.0;
    public double Megabytes => Bytes / (1024.0 * 1024.0);

    
    #region constructors
    
    public FileSize(long bytes)
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

    public bool Equals(FileSize? other)
    {
        return other is not null && Bytes == other.Bytes;
    }

    public override bool Equals(object? other)
    {
        return Equals(other as FileSize);
    }

    public override int GetHashCode()
    {
        return Bytes.GetHashCode();
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