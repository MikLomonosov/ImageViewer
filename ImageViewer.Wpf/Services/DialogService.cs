using System.Windows;
using ImageViewer.Application.Interfaces;

namespace ImageViewer.Wpf.Services;

public class DialogService : IDialogService
{
    public Task ShowInfoAsync(string message, string? title = null)
    {
        MessageBox.Show(message,
                title ?? "Информация",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

        return Task.CompletedTask;
    }

    public Task ShowWarningAsync(string message, string? title = null)
    {
        MessageBox.Show(message,
                title ?? "Предостережение",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        
        return Task.CompletedTask;
    }

    public Task ShowErrorAsync(string message, string? title = null)
    {
        MessageBox.Show(message,
                title ?? "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        
        return Task.CompletedTask;
    }

    public Task<bool> ConfirmAsync(string message, string? title = null)
    {
        var result = MessageBox.Show(message,
                                title ?? "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);


        return Task.FromResult(result == MessageBoxResult.Yes);
    }
}