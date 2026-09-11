using System.Windows;
using ImageViewer.Wpf.ViewModels;

namespace ImageViewer.Wpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        // System.Diagnostics.Debug.WriteLine($"[DIAG] DataContext type: {DataContext?.GetType().Name ?? "NULL"}");
        DataContext = mainViewModel;
        
    }
}