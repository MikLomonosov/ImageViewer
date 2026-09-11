using System.Windows;
using ImageViewer.Application.Interfaces;

namespace ImageViewer.Wpf.Services;

public class DialogService : IDialogService
{
    public void ShowInfo(string message, string? title = null)
    {
        MessageBox.Show(message,
                title ?? "Информация",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
    }

    public void ShowWarning(string message, string? title = null)
    {
        MessageBox.Show(message,
                title ?? "Предостережение",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
    }

    public void ShowError(string message, string? title = null)
    {
        MessageBox.Show(message,
                title ?? "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
    }

    public bool Confirm(string message, string? title = null)
    {
        var result = MessageBox.Show(message,
                                title ?? "Подтверждение",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);


        return result == MessageBoxResult.Yes;
    }
}