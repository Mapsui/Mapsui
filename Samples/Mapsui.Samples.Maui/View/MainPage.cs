using CommunityToolkit.Maui.Markup;
using Mapsui.Samples.Common;
using Mapsui.Samples.Maui.ViewModel;
using Mapsui.UI.Maui;

namespace Mapsui.Samples.Maui.View;

public sealed class MainPage : ContentPage, IDisposable
{
    readonly CollectionView _collectionView;
    readonly Picker _categoryPicker;
    readonly MapControl _mapControl = new();
    const int _menuItemWidth = 220;

    public MainPage(MainViewModel mainViewModel)
    {
        _categoryPicker = CreatePicker(mainViewModel);
        _collectionView = CreateCollectionView(mainViewModel);

        BindingContext = mainViewModel;
        _mapControl.Bind(MapControl.MapProperty, nameof(MainViewModel.Map));

        if (_mapControl.Map is Map map)
            map.Navigator.RotationLock = false;

        // Workaround. Samples need the MapControl in the current setup.
        mainViewModel.MapControl = _mapControl;

        Content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }
            },
            Children =
            {
                new ScrollView()
                {
                    WidthRequest = _menuItemWidth + 40,
                    Content = new VerticalStackLayout()
                    {
                        WidthRequest = _menuItemWidth + 20,
                        Spacing = 20,
                        Children =
                        {
                            _categoryPicker,
                            _collectionView
                        }
                    }
                }.Column(0).Padding(10),
                _mapControl.Column(1)
            }
        };
    }

    private static Picker CreatePicker(MainViewModel mainViewModel)
    {
        return new Picker
        {
            WidthRequest = _menuItemWidth,
            ItemsSource = mainViewModel.Categories
        }
        .Bind(Picker.SelectedItemProperty, nameof(mainViewModel.SelectedCategory))
        .Invoke(picker => picker.SelectedIndexChanged += mainViewModel.Picker_SelectedIndexChanged);
    }

    private static CollectionView CreateCollectionView(MainViewModel mainViewModel)
    {
        return new CollectionView
        {
            Margin = 4,
            ItemTemplate = new DataTemplate(CreateCollectionViewTemplate),
            SelectionMode = SelectionMode.Single,
            ItemsSource = mainViewModel.Samples
        }
        .Bind(SelectableItemsView.SelectedItemProperty, nameof(mainViewModel.SelectedSample))
        .Invoke(collectionView => collectionView.SelectionChanged += mainViewModel.CollectionView_SelectionChanged);
    }

    private static IView CreateCollectionViewTemplate()
    {
        return new Border
        {
            Padding = 10,
            Margin = new Thickness(2, 2),
            WidthRequest = _menuItemWidth,
            Content = new Label
            {
            }.Bind(Label.TextProperty, nameof(ISample.Name))
        };
    }

    public void Dispose()
    {
        _mapControl.Dispose();
    }
}
