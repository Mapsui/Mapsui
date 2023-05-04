using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapsui.Nts;
using Mapsui.Styles;
#if __MAUI__
using Mapsui.UI.Maui;
using Mapsui.UI.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

using Color = Microsoft.Maui.Graphics.Color;
using KnownColor = Mapsui.UI.Maui.KnownColor;
#else
using Mapsui.UI.Forms;
using Mapsui.UI.Forms.Extensions;
using Xamarin.Forms;

using Color = Xamarin.Forms.Color;
using KnownColor = Xamarin.Forms.Color;
#endif

namespace Mapsui.UI.Objects;

/// <summary>
/// Base class for all drawables like polyline, polygon and circle
/// </summary>
public class Drawable : BindableObject, IClickable, IFeatureProvider
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

    internal void HandleClicked(DrawableClickedEventArgs e)
    {
        Clicked?.Invoke(this, e);
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
