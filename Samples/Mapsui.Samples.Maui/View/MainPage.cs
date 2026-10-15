using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Controls.Shapes;
using Mapsui.Logging;
using Mapsui.Samples.Common;
using Mapsui.Samples.Maui.Model;
using Mapsui.Samples.Maui.ViewModel;
using Mapsui.UI.Maui;
using SkiaSharp;
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.Samples.Maui.View;

public sealed class MainPage : ContentPage, IDisposable
{
    private const int _menuItemWidth = 260;
    private readonly MainViewModel _viewModel;
    private readonly CollectionView _collectionView;
    private readonly Picker _categoryPicker;
    private readonly Picker _backendPicker;
    private readonly ContentView _mapHost = new();
    private readonly Label _backendSummary = new() { FontAttributes = FontAttributes.Bold, FontSize = 12, LineBreakMode = LineBreakMode.TailTruncation, HorizontalTextAlignment = TextAlignment.End, VerticalTextAlignment = TextAlignment.Center };
    private readonly Button _renderingButton = new() { Text = "⌄", FontSize = 20, Padding = 0, WidthRequest = 28, HeightRequest = 36, BorderWidth = 0, BackgroundColor = Colors.Transparent };
    private readonly VerticalStackLayout _renderingContent = new() { Spacing = 8, IsVisible = false };
    private readonly Border _renderingCard = new() { WidthRequest = _menuItemWidth, Padding = 12, StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) } };
    private readonly Label _statusLabel = new() { Text = "Loading map…", WidthRequest = _menuItemWidth, LineBreakMode = LineBreakMode.CharacterWrap };
    private readonly Label _controlBackendLabel = CreateDetailValue();
    private readonly Label _mauiViewLabel = CreateDetailValue();
    private readonly Label _nativeViewLabel = CreateDetailValue();
    private readonly Label _skiaPathLabel = CreateDetailValue();
    private readonly Label _gpuDeviceLabel = CreateDetailValue();
    private readonly SampleSwitchCoordinator _switches = new();
    private SampleScene? _scene;
    private Window? _window;
    private bool _loaded;
    private bool _disposed;
    private bool _showDiagnostics;

    public MainPage(MainViewModel mainViewModel)
    {
        Shell.SetNavBarIsVisible(this, false);
        _backendSummary.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#176877"), Color.FromArgb("#70C8D6"));
        _renderingButton.SetAppThemeColor(Button.TextColorProperty, Color.FromArgb("#6E6E6E"), Color.FromArgb("#ACACAC"));
        _viewModel = mainViewModel;
        _categoryPicker = CreatePicker(mainViewModel);
        _collectionView = CreateCollectionView(mainViewModel);
        var backends = new List<RenderBackend> { RenderBackend.Software, RenderBackend.Gpu };
        if (DeviceInfo.Platform == DevicePlatform.Android)
            backends.Add(RenderBackend.AndroidSurfaceView);
        _backendPicker = new Picker
        {
            ItemsSource = backends,
            SelectedItem = backends.Contains(MapControl.Backend) ? MapControl.Backend : RenderBackend.Gpu
        };
        _renderingContent.Children.Add(CreateDivider());
        _renderingContent.Children.Add(new Label { Text = "Backend", FontSize = 12, FontAttributes = FontAttributes.Bold });
        _renderingContent.Children.Add(_backendPicker);
        _renderingContent.Children.Add(CreateMutedLabel("Changing this creates a new map control."));
        _renderingContent.Children.Add(CreateDivider());
        _renderingContent.Children.Add(CreateMutedLabel("Control details", FontAttributes.Bold));
        _renderingContent.Children.Add(CreateDetail("Control backend", _controlBackendLabel));
        _renderingContent.Children.Add(CreateDetail("MAUI view", _mauiViewLabel));
        _renderingContent.Children.Add(CreateDetail("Native view", _nativeViewLabel));
        _renderingContent.Children.Add(CreateDetail("Skia graphics API", _skiaPathLabel));
        _renderingContent.Children.Add(CreateDetail("GPU device", _gpuDeviceLabel));
        var renderingHeader = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Auto } },
            Children =
            {
                new Label { Text = "Rendering", FontAttributes = FontAttributes.Bold, VerticalTextAlignment = TextAlignment.Center }.Column(0),
                _backendSummary.Column(1),
                _renderingButton.Column(2)
            }
        };
        _renderingCard.Content = new VerticalStackLayout
        {
            Spacing = 8,
            Children = { renderingHeader, _renderingContent }
        };
        UpdateRenderingHeader();
        BindingContext = mainViewModel;
        Content = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = GridLength.Star } },
            Children =
            {
                new ScrollView
                {
                    WidthRequest = _menuItemWidth + 40,
                    Content = new VerticalStackLayout
                    {
                        WidthRequest = _menuItemWidth + 20,
                        Spacing = 20,
                        Children = { _renderingCard, _statusLabel, _categoryPicker, _collectionView }
                    }
                }.Column(0).Padding(10),
                _mapHost.Column(1)
            }
        };
        Loaded += OnLoaded;
        _viewModel.SampleChanged += OnSampleChanged;
        _backendPicker.SelectedIndexChanged += OnBackendChanged;
        _renderingButton.Clicked += OnRenderingClicked;
    }

    private void OnLoaded(object? sender, EventArgs args)
    {
        if (_loaded || _disposed)
            return;
        _loaded = true;
        _window = Window;
        if (_window != null)
            _window.Destroying += OnWindowDestroying;
        // The native handler and GPU context may appear after the scene is attached.
        Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            if (_disposed)
                return false;
            if (_showDiagnostics && _scene != null)
                UpdateDiagnostics(_scene.Control);
            return true;
        });
        RequestScene(false);
    }

    private void OnWindowDestroying(object? sender, EventArgs args) => Dispose();
    private void OnSampleChanged(object? sender, EventArgs args) => RequestScene(false);
    private void OnBackendChanged(object? sender, EventArgs args) => RequestScene(true);
    private void OnRenderingClicked(object? sender, EventArgs args)
    {
        _renderingContent.IsVisible = !_renderingContent.IsVisible;
        UpdateRenderingHeader();
    }

    private void UpdateRenderingHeader()
    {
        _backendSummary.Text = _scene?.Control.ActiveBackend.ToString()
            ?? (_loaded ? "Unavailable" : _backendPicker.SelectedItem?.ToString() ?? "Unavailable");
        _renderingButton.Text = _renderingContent.IsVisible ? "⌃" : "⌄";
        SemanticProperties.SetDescription(_renderingButton, _renderingContent.IsVisible ? "Collapse rendering settings" : "Expand rendering settings");
    }

    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "UI selection entrypoint; exceptions are reported here.")]
    private async void RequestScene(bool preserveViewport)
    {
        if (!_loaded || _disposed || _viewModel.SelectedSample is not { } selected || _backendPicker.SelectedItem is not RenderBackend backend)
            return;
        _showDiagnostics = false;
        _statusLabel.Text = $"Loading {backend}…";
        _statusLabel.IsVisible = true;
        _controlBackendLabel.Text = "Creating control…";
        _mauiViewLabel.Text = "Waiting…";
        _nativeViewLabel.Text = "Waiting…";
        _skiaPathLabel.Text = "Waiting for first paint…";
        _gpuDeviceLabel.Text = "Waiting…";
        var viewport = preserveViewport && _scene?.IsSameSample(selected) == true ? _scene.Control.Map.Navigator.Viewport : (Viewport?)null;
        try
        {
            await _switches.RunAsync(async cancellationToken =>
            {
                var next = await SampleScene.CreateAsync(selected, backend, viewport, cancellationToken);
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var previous = _scene;
                    DetachScene();
                    _scene = null;
                    previous?.Dispose();
                    _viewModel.MapControl = next.Control;
                    _viewModel.Map = next.Control.Map;
                    next.Control.BindingContext = _viewModel;
                    next.Control.Bind(MapControl.MapProperty, nameof(MainViewModel.Map));
                    _mapHost.Content = next.Control;
                    _scene = next;
                    _showDiagnostics = true;
                    UpdateRenderingHeader();
                    UpdateDiagnostics(next.Control);
                    _statusLabel.IsVisible = false;
                }
                catch
                {
                    next.Control.RemoveBinding(MapControl.MapProperty);
                    next.Control.BindingContext = null;
                    if (ReferenceEquals(_mapHost.Content, next.Control))
                        _mapHost.Content = null;
                    if (ReferenceEquals(_viewModel.MapControl, next.Control))
                    {
                        _viewModel.MapControl = null;
                        _viewModel.Map = null;
                    }
                    if (ReferenceEquals(_scene, next))
                        _scene = null;
                    _showDiagnostics = false;
                    next.Dispose();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
            if (!_disposed)
            {
                if (_scene is { } current)
                {
                    _showDiagnostics = true;
                    UpdateDiagnostics(current.Control);
                }
                else
                {
                    _controlBackendLabel.Text = "Unavailable";
                    _mauiViewLabel.Text = "Unavailable";
                    _nativeViewLabel.Text = "Unavailable";
                    _skiaPathLabel.Text = "Unavailable";
                    _gpuDeviceLabel.Text = "Unavailable";
                }
                UpdateRenderingHeader();
                _statusLabel.Text = $"Could not load {backend}: {ex.Message}";
            }
        }
    }

    private void UpdateDiagnostics(MapControl control)
    {
        var view = control.Content;
        var nativeView = view?.Handler?.PlatformView?.GetType().Name ?? "Waiting for native view";
        var graphicsApi = control.ActiveBackend == RenderBackend.Software
            ? "CPU"
            : control.Map.RenderService.GpuContext is GRContext context
                ? context.Backend.ToString()
                : "Waiting for GPU paint";
        _controlBackendLabel.Text = control.ActiveBackend.ToString();
        _mauiViewLabel.Text = view?.GetType().Name ?? "Waiting for MAUI view";
        _nativeViewLabel.Text = nativeView;
        _skiaPathLabel.Text = graphicsApi;
        _gpuDeviceLabel.Text = control.ActiveBackend == RenderBackend.Software ? "Not applicable" : "Not identified";
    }

    private static Label CreateDetailValue() => new() { FontSize = 11, FontAttributes = FontAttributes.Bold, LineBreakMode = LineBreakMode.NoWrap, HorizontalTextAlignment = TextAlignment.End, VerticalTextAlignment = TextAlignment.Center };

    private static Label CreateMutedLabel(string text, FontAttributes fontAttributes = FontAttributes.None)
    {
        var label = new Label { Text = text, FontSize = 11, FontAttributes = fontAttributes };
        label.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#6E6E6E"), Color.FromArgb("#ACACAC"));
        return label;
    }

    private static BoxView CreateDivider()
    {
        var divider = new BoxView { HeightRequest = 1 };
        divider.SetAppThemeColor(BoxView.ColorProperty, Color.FromArgb("#C8C8C8"), Color.FromArgb("#404040"));
        return divider;
    }

    private static Grid CreateDetail(string title, Label value)
    {
        var label = CreateMutedLabel(title);
        label.LineBreakMode = LineBreakMode.TailTruncation;
        label.VerticalTextAlignment = TextAlignment.Center;
        return new Grid
        {
            MinimumHeightRequest = 20,
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Auto } },
            Children = { label.Column(0), value.Column(1) }
        };
    }

    private void DetachScene()
    {
        if (_scene == null)
            return;
        _scene.Control.RemoveBinding(MapControl.MapProperty);
        _scene.Control.BindingContext = null;
        _mapHost.Content = null;
        _viewModel.MapControl = null;
        _viewModel.Map = null;
    }

    private static Picker CreatePicker(MainViewModel mainViewModel) => new Picker
    {
        WidthRequest = _menuItemWidth,
        ItemsSource = mainViewModel.Categories
    }
    .Bind(Picker.SelectedItemProperty, nameof(mainViewModel.SelectedCategory))
    .Invoke(picker => picker.SelectedIndexChanged += mainViewModel.Picker_SelectedIndexChanged);

    private static CollectionView CreateCollectionView(MainViewModel mainViewModel) => new CollectionView
    {
        Margin = 4,
        ItemTemplate = new DataTemplate(CreateCollectionViewTemplate),
        SelectionMode = SelectionMode.Single,
        ItemsSource = mainViewModel.Samples
    }
    .Bind(SelectableItemsView.SelectedItemProperty, nameof(mainViewModel.SelectedSample))
    .Invoke(collectionView => collectionView.SelectionChanged += mainViewModel.CollectionView_SelectionChanged);

    private static IView CreateCollectionViewTemplate() => new Border
    {
        Padding = 10,
        Margin = new Thickness(2, 2),
        WidthRequest = _menuItemWidth,
        Content = new Label().Bind(Label.TextProperty, nameof(ISample.Name))
    };

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _switches.Dispose();
        Loaded -= OnLoaded;
        _viewModel.SampleChanged -= OnSampleChanged;
        _backendPicker.SelectedIndexChanged -= OnBackendChanged;
        _renderingButton.Clicked -= OnRenderingClicked;
        _categoryPicker.SelectedIndexChanged -= _viewModel.Picker_SelectedIndexChanged;
        _collectionView.SelectionChanged -= _viewModel.CollectionView_SelectionChanged;
        if (_window != null)
            _window.Destroying -= OnWindowDestroying;
        DetachScene();
        _scene?.Dispose();
        _scene = null;
    }
}
