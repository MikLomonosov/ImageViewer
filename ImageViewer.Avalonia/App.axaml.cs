using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ImageViewer.Application.Configuration;
using ImageViewer.Application.Interfaces;
using ImageViewer.Application.UseCases;
using ImageViewer.Avalonia.Services;
using ImageViewer.Avalonia.ViewModels;
using ImageViewer.Domain.Repositories;
using ImageViewer.Infrastructure.ImageProcessing;
using ImageViewer.Infrastructure.Repositories;
using ImageViewer.Infrastructure.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImageViewer.Avalonia;

public partial class App : global::Avalonia.Application
{
    private IHost? _host;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) => ConfigureServices(services, mainWindow))
                .Build();
            
            _host.Start();
            
            mainWindow.DataContext = _host.Services.GetRequiredService<MainViewModel>();
            desktop.MainWindow = mainWindow;
            
            desktop.ShutdownRequested += (_, _) => _host.StopAsync().GetAwaiter().GetResult();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services, MainWindow mainWindow)
    {
        services.Configure<ImageLoadingLimits>(opts =>
        {
            opts.MaxFileSizeBytes = 50 * 1024 * 1024;
            opts.MaxDegreeOfParallelism = 4;
        });

        services.AddSingleton<IImageLoaderService, ImageLoaderService>();
        services.AddSingleton<IThumbnailService, ThumbnailService>();
        services.AddSingleton<IImageSerializer, BinaryImageSerializer>();
        services.AddSingleton<IImageCollectionRepository, ImageCollectionRepository>();
        services.AddSingleton<LoadImagesUseCase>();
        services.AddSingleton<SerializeCollectionUseCase>();
        services.AddSingleton<DeserializeCollectionUseCase>();
        services.AddSingleton<IFileDialogService>(new FileDialogService(mainWindow));
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<MainViewModel>();
    }
}