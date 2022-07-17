using CommunityToolkit.Maui.Markup;
using Mapsui.Extensions;
using Mapsui.Samples.Common;
using Mapsui.Tiling;
using Mapsui.UI.Maui;

namespace Mapsui.Samples.Maui;

public sealed class MainPage : ContentPage, IDisposable
{
    IEnumerable<ISampleBase> allSamples;
    CollectionView sampleCollectionView = CreateCollectionView();
    Picker categoryPicker = CreatePicker();
    MapControl mapControl = new MapControl();

    public MainPage()
	{
        sampleCollectionView.SelectionChanged += CollectionView_SelectionChanged;
        mapControl.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());

        Content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }
            },
            Children =
            {
                new VerticalStackLayout()
                {
                    Spacing = 20,
                    Children =
                    {
                        categoryPicker,
                        sampleCollectionView
                    }
                }.Column(0).Padding(20),
                mapControl.Column(1)
            }
        };

        allSamples = AllSamples.GetSamples() ?? new List<ISampleBase>();
        var categories = allSamples.Select(s => s.Category).Distinct().OrderBy(c => c);
        categoryPicker!.ItemsSource = categories.ToList();
        categoryPicker.SelectedIndexChanged += categoryPicker_SelectedIndexChanged;
        categoryPicker.SelectedItem = "Info";
    }

    private static Picker CreatePicker()
    {
        return new Picker
        {
            WidthRequest = 220,
        };
    }

    private static CollectionView CreateCollectionView()
    {
        return new CollectionView
        {
            ItemTemplate = new DataTemplate(() => CreateCollectionViewTemplate()),
            SelectionMode = SelectionMode.Single,
        };
    }

    private static IView CreateCollectionViewTemplate()
    {
        return new Frame
        {
            BorderColor = Color.FromArgb("#DDDDDD"),
            HasShadow = true,
            CornerRadius = 4,
            Padding = 10,
            Margin = new Thickness(0, 2),
            Content = new Label
            {
                WidthRequest = 200,
                        
            }.Bind(Label.TextProperty, nameof(ISample.Name))
        };
    }

    private void CollectionView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Catch.Exceptions(async () =>
        {
            if (e.CurrentSelection == null)
            {
                return;
            }

            var sample = (ISample)e.CurrentSelection[0];
            mapControl.Map = await sample.CreateMapAsync();
        });
    }

    private void categoryPicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        FillListWithSamples();
    }

    private void FillListWithSamples()
    {
        var selectedCategory = categoryPicker.SelectedItem?.ToString() ?? "";
        sampleCollectionView.ItemsSource = allSamples.Where(s => s.Category == selectedCategory);
    }

    public void Dispose()
    {
        mapControl.Dispose();
    }
}
