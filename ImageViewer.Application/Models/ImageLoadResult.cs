using ImageViewer.Domain.Entities;

namespace ImageViewer.Application.Models;

public sealed record ImageLoadResult(IReadOnlyList<Image> LoadedImages, IReadOnlyList<ImageLoadError> Errors);