namespace ImageViewer.Application.Interfaces;

public interface IFileDialogService
{
    IReadOnlyList<string> OpenImageFilesDialog();

    string? SaveDocumentDialog(string fileName, string filter);
    string? OpenDocumentDialog(string filter);
}