using Xamarin.Forms;

namespace Mapsui.Samples.Forms;

public partial class LeaksPage : ContentPage
{
    public static bool DisposeMapView = true;

    public LeaksPage(string report)
    {
        InitializeComponent();
        label.Text = report;
        enableDisposeCheckbox.IsChecked = DisposeMapView;
        enableDisposeCheckbox.CheckedChanged += (s, e) => DisposeMapView = e.Value;
    }
}
