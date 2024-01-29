using Mapsui;

namespace AvaloniaApplication1.ViewModels;

public class MainViewModel : ViewModelBase
{
    public string Greeting => "Welcome to Avalonia!";

    public Map BoundMap { get; set; } = new Map();
}
