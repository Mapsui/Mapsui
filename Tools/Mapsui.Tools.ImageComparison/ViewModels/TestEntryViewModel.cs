using System;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace Mapsui.Tools.ImageComparison.ViewModels;

sealed class TestEntryViewModel : ReactiveObject, IDisposable
{
    Bitmap? _thumbnail;

    public TestEntryViewModel(string name) => Name = name;

    public string Name { get; }

    public Bitmap? Thumbnail
    {
        get => _thumbnail;
        set
        {
            _thumbnail?.Dispose();
            this.RaiseAndSetIfChanged(ref _thumbnail, value);
        }
    }

    public void Dispose() => _thumbnail?.Dispose();
}
