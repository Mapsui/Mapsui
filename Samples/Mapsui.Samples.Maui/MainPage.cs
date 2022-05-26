namespace Mapsui.Samples.Maui;

public class MainPage : ContentPage
{
	public MainPage()
	{
        var mapControl = new Mapsui.UI.Maui.MapControl();

        mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

        Content = mapControl;
	}
}