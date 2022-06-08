using CommunityToolkit.Maui.Markup;
using Mapsui.Samples.Common;
using Mapsui.Tiling;
using Mapsui.UI.Maui;

namespace Mapsui.Samples.Maui;

public class MainPage : ContentPage
{
    IEnumerable<ISampleBase> allSamples;
    CollectionView collectionView = CreateCollectionView();
    Picker sampleCategoryPicker = new Picker();
    MapControl mapControl = new MapControl();

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
            IsClippedToBounds = true,
            Content = new HorizontalStackLayout
            {
                Margin = 0,
                Padding = 0,
                Children =
                {
                    new Label{ Style = CreateLabelStyle() }.Bind(Label.TextProperty, nameof(ISample.Name)),
                }
            }
        };
    }

    private static Style CreateLabelStyle()
    {
        var style = new Style(typeof(Label));
        style.Setters.Add(new Setter { Property = Label.VerticalOptionsProperty, Value = LayoutOptions.Center });
        style.Setters.Add(new Setter { Property = Label.HorizontalOptionsProperty, Value = LayoutOptions.Start });
        style.Setters.Add(new Setter { Property = Label.HorizontalTextAlignmentProperty, Value = TextAlignment.Start });
        style.Setters.Add(new Setter { Property = Label.WidthRequestProperty, Value = 200 });
        return style;
    }


    public MainPage()
	{
        collectionView.SelectionChanged += CollectionView_SelectionChanged;
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
                        sampleCategoryPicker,
                        collectionView
                    }
                }.Column(0).Padding(20),
                mapControl.Column(1)
            }
        };

        allSamples = AllSamples.GetSamples() ?? new List<ISampleBase>();
        var categories = allSamples.Select(s => s.Category).Distinct().OrderBy(c => c);
        sampleCategoryPicker!.ItemsSource = categories.ToList();
        sampleCategoryPicker.SelectedIndexChanged += SampleCategoryPicker_SelectedIndexChanged; ;
        sampleCategoryPicker.SelectedItem = "Info";
    }

    private void CollectionView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection == null)
        {
            return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
        }

        var sample = ((ISample)e.CurrentSelection[0]);
        sample.Setup(mapControl);
    }

    private void SampleCategoryPicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        FillListWithSamples();
    }

    private void FillListWithSamples()
    {
        var selectedCategory = sampleCategoryPicker.SelectedItem?.ToString() ?? "";
        collectionView.ItemsSource = allSamples.Where(s => s.Category == selectedCategory);
    }
}
