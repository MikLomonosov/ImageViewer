using System.Windows;
using ImageViewer.Application.Configuration;
using ImageViewer.Application.Interfaces;
using ImageViewer.Application.UseCases;
using ImageViewer.Domain.Repositories;
using ImageViewer.Infrastructure.ImageProcessing;
using ImageViewer.Infrastructure.Repositories;
using ImageViewer.Infrastructure.Serialization;
using ImageViewer.Wpf.Converters;
using ImageViewer.Wpf.Services;
using ImageViewer.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImageViewer.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;
    
    #region constructors

    public App()
    {
        Resources["BoolToVisibilityConverter"] = new BoolToVisibilityConverter();
    }
    
    #endregion
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(ConfigureServices)
            .Build();

        _host.Start();
        
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        
        base.OnExit(e);
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        services.Configure<ImageLoadingLimits>(opts =>
        {
            opts.MaxFileSizeBytes = 50 * 1024 * 1024;
        });
        
        services.AddSingleton<IImageLoaderService, ImageLoaderService>();
        services.AddSingleton<LoadImagesUseCase>();
        services.AddSingleton<IFileDialogService, FileDialogService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IThumbnailService, ThumbnailService>();
        services.AddSingleton<IImageSerializer, BinaryImageSerializer>();
        services.AddSingleton<IImageCollectionRepository, ImageCollectionRepository>();
        services.AddSingleton<SerializeCollectionUseCase>();
        services.AddSingleton<DeserializeCollectionUseCase>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
    }
}