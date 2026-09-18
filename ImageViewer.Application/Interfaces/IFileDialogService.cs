namespace ImageViewer.Application.Interfaces;

public interface IFileDialogService
{
    Task<IReadOnlyList<string>> OpenImageFilesDialogAsync();

    Task<string?> SaveDocumentDialogAsync(string fileName, string filter);
    Task<string?> OpenDocumentDialogAsync(string filter);
}