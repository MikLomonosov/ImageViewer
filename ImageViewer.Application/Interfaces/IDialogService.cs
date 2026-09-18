namespace ImageViewer.Application.Interfaces;

public interface IDialogService
{
    Task ShowInfoAsync(string message, string? title = null);
    Task ShowWarningAsync(string message, string? title = null);
    Task ShowErrorAsync(string message, string? title = null);
    Task<bool> ConfirmAsync(string message, string? title = null);
}