using System;
using Avalonia.Media.Imaging;
using ImageViewer.Domain.Entities;
using ImageViewer.Domain.ValueObjects;
using ImageViewer.Avalonia.ViewModels.Base;

namespace ImageViewer.Avalonia.ViewModels;

public sealed class ImageItemViewModel : BaseViewModel
{
    private readonly Image _image;
    private Bitmap? _thumbnail;
    private bool _isSelected;
    
    public Guid Id => _image.Id;
    public string FileName => _image.Name;
    public string Path => _image.Path;
    public string SizeFormatted => _image.Size.ToString();
    public string CreatedDateFormatted => _image.CreatedDateUtc.ToLocalTime().ToString("g");
    public string DimensionsFormatted => _image.Dimensions?.ToString() ?? "-";

    public Bitmap? Thumbnail => _thumbnail;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
    
    #region constructors

    public ImageItemViewModel(Image image)
    {
        _image = image ?? throw new ArgumentNullException(nameof(image));
        _thumbnail = _image.Thumbnail is not null ? CreateBitmap(_image.Thumbnail) : null;
    }
    
    #endregion

    private static Bitmap CreateBitmap(ImageBinaryData imageBinaryData)
    {
        using var stream = imageBinaryData.OpenRead();
        
        return new Bitmap(stream);
    }

    
}