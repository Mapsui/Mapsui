using CommunityToolkit.Maui.Markup;
using Mapsui.Samples.Common;
using Mapsui.Tiling;
using Mapsui.UI.Maui;

namespace Mapsui.Samples.Maui;

public class MainPage : ContentPage
{
    readonly IEnumerable<ISampleBase> allSamples;

    public MainPage()
	{
        MapControl.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());

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
                        SampleCategoryPicker,
                        SampleList
                    }
                }.Column(0).Padding(20),
                MapControl.Column(1)
            }
        };

        SampleList.ItemSelected += SampleList_ItemSelected;

        allSamples = AllSamples.GetSamples() ?? new List<ISampleBase>();
        var categories = allSamples.Select(s => s.Category).Distinct().OrderBy(c => c);
        SampleCategoryPicker!.ItemsSource = categories.ToList<string>();
        SampleCategoryPicker.SelectedIndexChanged += SampleCategoryPicker_SelectedIndexChanged; ;
        SampleCategoryPicker.SelectedItem = "Info";
    }

    private void SampleList_ItemSelected(object? sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem == null)
        {
            return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
        }

        var sampleName = e.SelectedItem.ToString();
        var baseSample = allSamples.FirstOrDefault(x => x.Name == sampleName);

        if (baseSample is ISample sample)
            sample.Setup(MapControl);
    }

    public ListView SampleList { get; set; } = new ListView();
    public Picker SampleCategoryPicker { get; set; } = new Picker();

    public MapControl MapControl = new MapControl();

    private void SampleCategoryPicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        FillListWithSamples();
    }

    private void FillListWithSamples()
    {
        var selectedCategory = SampleCategoryPicker.SelectedItem?.ToString() ?? "";
        SampleList.ItemsSource = allSamples.Where(s => s.Category == selectedCategory).Select(x => x.Name);
    }
}
