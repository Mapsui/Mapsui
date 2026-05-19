using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Mapsui.Tools.ImageComparison.ViewModels;

namespace Mapsui.Tools.ImageComparison.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = new MainViewModel(PickFolderAsync);
    }

    async Task<string?> PickFolderAsync()
    {
        var results = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select root folder (Tests/Mapsui.Rendering.Skia.Tests)",
            AllowMultiple = false,
        });
        return results.FirstOrDefault()?.Path.LocalPath;
    }
}
