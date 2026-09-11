using System.IO;
using System.Windows.Media.Imaging;
using ImageViewer.Domain.Entities;
using ImageViewer.Wpf.ViewModels.Base;

namespace ImageViewer.Wpf.ViewModels;

public sealed class ImageItemViewModel : BaseViewModel
{
    private readonly Image _image;
    private BitmapImage? _thumbnailSource;
    private bool _isSelected;
    
    public Guid Id => _image.Id;
    public string FileName => _image.Name;
    public string Path => _image.Path;
    public string SizeFormatted => _image.Size.ToString();
    public string CreatedDateFormatted => _image.CreatedDateUtc.ToLocalTime().ToString("g");
    public string DimensionsFormatted => _image.Dimensions?.ToString() ?? "-";

    public BitmapImage? Thumbnail
    {
        get
        {
            if (_thumbnailSource is null && _image.Thumbnail is not null)
                _thumbnailSource = CreateBitmapImage(_image.Thumbnail.ToArray());
            
            return _thumbnailSource;
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
    
    #region constructors

    public ImageItemViewModel(Image image)
    {
        _image = image ?? throw new ArgumentNullException(nameof(image));
    }
    
    #endregion

    private static BitmapImage CreateBitmapImage(byte[] bytes)
    {
        var image = new BitmapImage();
        
        using var stream = new MemoryStream(bytes);
        
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad; // load to memory and close the stream
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze(); //thread safety
        
        return image;
    }

    
}