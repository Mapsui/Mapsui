using CommunityToolkit.Maui.Markup;
using Mapsui.Extensions;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Samples.CustomWidget;
using Mapsui.Samples.Maui.ViewModel;
using Mapsui.Tiling;
using Mapsui.UI.Maui;

namespace Mapsui.Samples.Maui.View;

public sealed class MainPage : ContentPage, IDisposable
{
    readonly CollectionView collectionView;
    readonly Picker categoryPicker;
    readonly MapControl mapControl = new MapControl();

    public MainPage(MainViewModel mainViewModel)
    {
        categoryPicker = CreatePicker(mainViewModel);
        collectionView = CreateCollectionView(mainViewModel);

        BindingContext = mainViewModel;
        mapControl.SetBinding(MapControl.MapProperty, new Binding(nameof(MainViewModel.Map)));

        // Workaround. Samples need the MapControl in the current setup.
        mainViewModel.MapControl = mapControl;

        // The CustomWidgetSkiaRenderer needs to be registered to make the CustomWidget sample work.
        // Perhaps it is possible to let the sample itself do this so we do not have to do this for each platform.
        mapControl.Renderer.WidgetRenders[typeof(CustomWidget.CustomWidget)] = new CustomWidgetSkiaRenderer();

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
                        collectionView
                    }
                }.Column(0).Padding(20),
                mapControl.Column(1)
            }
        };
    }

    private static Picker CreatePicker(MainViewModel mainViewModel)
    {
        return new Picker
        {
            WidthRequest = 220,
            ItemsSource = mainViewModel.Categories
        }
        .Bind(Picker.SelectedItemProperty, nameof(mainViewModel.SelectedCategory))
        .Invoke(picker => picker.SelectedIndexChanged += mainViewModel.Picker_SelectedIndexChanged);
    }

    private static CollectionView CreateCollectionView(MainViewModel mainViewModel)
    {
        return new CollectionView
        {
            ItemTemplate = new DataTemplate(() => CreateCollectionViewTemplate()),
            SelectionMode = SelectionMode.Single,
            ItemsSource = mainViewModel.Samples
        }
        .Bind(SelectableItemsView.SelectedItemProperty, nameof(mainViewModel.SelectedSample))
        .Invoke(collectionView => collectionView.SelectionChanged += mainViewModel.CollectionView_SelectionChanged);
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

    public void Dispose()
    {
        mapControl.Dispose();
    }
}
