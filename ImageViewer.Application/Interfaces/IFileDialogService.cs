namespace ImageViewer.Application.Interfaces;

public interface IFileDialogService
{
    IReadOnlyCollection<string> OpenImageFilesDialog();
}