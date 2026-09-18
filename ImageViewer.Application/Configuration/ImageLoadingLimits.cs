namespace ImageViewer.Application.Configuration;

public sealed class ImageLoadingLimits
{
    public long MaxFileSizeBytes { get; set; } = 50 * 1024 * 1024; // 50 MB default
    public int MaxDegreeOfParallelism { get; set; } = 4; 
}