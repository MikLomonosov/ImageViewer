namespace ImageViewer.Application.Models;

public sealed record ImageLoadError(string FilePath, string Reason);