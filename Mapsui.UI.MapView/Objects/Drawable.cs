using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapsui.Nts;
using Mapsui.Nts.Providers;
using Mapsui.Styles;

#if __MAUI__
namespace Mapsui.UI.Maui;
#elif __UWP__
namespace Mapsui.UI.Uwp;
#elif __ANDROID__ && !HAS_UNO_WINUI
namespace Mapsui.UI.Android;
#elif __IOS__ && !HAS_UNO_WINUI && !__FORMS__
namespace Mapsui.UI.iOS;
#elif __WINUI__
namespace Mapsui.UI.WinUI;
#elif __FORMS__
namespace Mapsui.UI.Forms;
#elif __AVALONIA__
namespace Mapsui.UI.Avalonia;
#elif __ETO_FORMS__
namespace Mapsui.UI.Eto;
#elif __BLAZOR__
namespace Mapsui.UI.Blazor;
#elif __WPF__
namespace Mapsui.UI.Wpf;
#else
namespace Mapsui.UI;
#endif

/// <summary>
/// Base class for all drawables like polyline, polygon and circle
/// </summary>
public class Drawable : BindableObject, IClickable, IFeatureProvider, IDrawable
{
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(nameof(Label), typeof(string), typeof(Pin), default(string));
    public static readonly BindableProperty StrokeWidthProperty = BindableProperty.Create(nameof(StrokeWidth), typeof(float), typeof(Circle), 1f);
    public static readonly BindableProperty MinVisibleProperty = BindableProperty.Create(nameof(MinVisible), typeof(double), typeof(Circle), 0.0);
    public static readonly BindableProperty MaxVisibleProperty = BindableProperty.Create(nameof(MaxVisible), typeof(double), typeof(Circle), double.MaxValue);
    public static readonly BindableProperty ZIndexProperty = BindableProperty.Create(nameof(ZIndex), typeof(int), typeof(Circle), 0);
    public static readonly BindableProperty IsClickableProperty = BindableProperty.Create(nameof(IsClickable), typeof(bool), typeof(Drawable), false);
    public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create(nameof(StrokeColor), typeof(Color), typeof(Circle), KnownColor.Black);

    /// <summary>
    /// Label of drawable
    /// </summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// StrokeWidth of drawable in pixel
    /// </summary>
    public float StrokeWidth
    {
        get => (float)GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    /// <summary>
    /// StrokeColor for drawable
    /// </summary>
    public Color StrokeColor
    {
        get { return (Color)GetValue(StrokeColorProperty); }
        set { SetValue(StrokeColorProperty, value); }
    }

    /// <summary>
    /// MinVisible for drawable in resolution of Mapsui (smaller values are higher zoom levels)
    /// </summary>
    public double MinVisible
    {
        get => (double)GetValue(MinVisibleProperty);
        set => SetValue(MinVisibleProperty, value);
    }

    /// <summary>
    /// MaxVisible for drawable in resolution of Mapsui (smaller values are higher zoom levels)
    /// </summary>
    public double MaxVisible
    {
        get => (double)GetValue(MaxVisibleProperty);
        set => SetValue(MaxVisibleProperty, value);
    }

    /// <summary>
    /// ZIndex of this drawable
    /// </summary>
    public int ZIndex
    {
        get => (int)GetValue(ZIndexProperty);
        set => SetValue(ZIndexProperty, value);
    }

    /// <summary>
    /// Is this drawable clickable
    /// </summary>
    public bool IsClickable
    {
        get => (bool)GetValue(IsClickableProperty);
        set => SetValue(IsClickableProperty, value);
    }

    /// <summary>
    /// Object for free use
    /// </summary>
    public object? Tag { get; set; }

    private GeometryFeature? feature;

    /// <summary>
    /// Mapsui Feature belonging to this drawable
    /// </summary>
    public GeometryFeature? Feature
    {
        get => feature;
        set
        {
            if (feature == null || !feature.Equals(value))
                feature = value;
        }
    }

    /// <summary>
    /// Event called, if this drawable is clicked an IsClickable is true
    /// </summary>
    public event EventHandler<DrawableClickedEventArgs>? Clicked;

    void IClickable.HandleClicked(DrawableClickedEventArgs e)
    {
    }

    void IDrawable.HandleClicked(IDrawableClicked e)
    {
        Clicked?.Invoke(this, (DrawableClickedEventArgs)e);
    }

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        var vectorStyle = ((VectorStyle?)Feature?.Styles.FirstOrDefault());
        if (vectorStyle == null || vectorStyle.Line == null)
            return;

        switch (propertyName)
        {
            case nameof(StrokeWidth):
                vectorStyle.Line.Width = StrokeWidth;
                break;
            case nameof(StrokeColor):
                vectorStyle.Line.Color = StrokeColor.ToMapsui();
                break;
            case nameof(MinVisible):
                vectorStyle.MinVisible = MinVisible;
                break;
            case nameof(MaxVisible):
                vectorStyle.MaxVisible = MaxVisible;
                break;
        }
    }
}
