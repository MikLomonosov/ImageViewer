namespace ImageViewer.Application.Interfaces;

public interface IFileDialogService
{
    IReadOnlyList<string> OpenImageFilesDialog();
}