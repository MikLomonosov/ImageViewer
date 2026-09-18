using System.Threading.Tasks;
using ImageViewer.Application.Interfaces;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace ImageViewer.Avalonia.Services;

public class DialogService : IDialogService
{
    public Task ShowInfoAsync(string message, string? title = null)
    {
        return ShowAsync(title ?? "Информация", message, Icon.Info, ButtonEnum.Ok);
    }

    public Task ShowWarningAsync(string message, string? title = null)
    {
        return ShowAsync(title ?? "Предупреждение", message, Icon.Warning, ButtonEnum.Ok);
    }

    public Task ShowErrorAsync(string message, string? title = null)
    {
        return ShowAsync(title ?? "Ошибка", message, Icon.Error, ButtonEnum.Ok);
    }

    public async Task<bool> ConfirmAsync(string message, string? title = null)
    {
        var showBox = MessageBoxManager.GetMessageBoxStandard(
            title ?? "Подтверждение", message, ButtonEnum.YesNo, Icon.Question);
        
        var result = await showBox.ShowAsync();

        return result == ButtonResult.Yes;
    }   

    private static async Task ShowAsync(string title, string message, Icon icon, ButtonEnum buttons)
    {
        var showBox = MessageBoxManager.GetMessageBoxStandard(title, message, buttons, icon);
        await showBox.ShowAsync();
    }
}