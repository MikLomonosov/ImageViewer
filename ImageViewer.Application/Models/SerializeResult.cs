namespace ImageViewer.Application.Models;

public record SerializeResult(bool Saved,
                                IReadOnlyList<string> SkippedOriginalPaths);
