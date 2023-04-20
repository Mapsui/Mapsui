global using CalloutStyle = Mapsui.Styles.CalloutStyle;

#if __MAUI__
global using Microsoft.Maui;
global using Microsoft.Maui.Graphics;
global using Microsoft.Maui.Controls;
global using SkiaSharp.Views.Maui;

global using Animation = Microsoft.Maui.Animation;
global using Color = Microsoft.Maui.Graphics.Color;
global using KnownColor = Mapsui.UI.Maui.KnownColor;
global using Point = Microsoft.Maui.Graphics.Point;
global using Rectangle = Microsoft.Maui.Graphics.Rect;
#elif __FORMS__
global using SkiaSharp.Views.Forms;
global using Xamarin.Forms;

global using Animation = Xamarin.Forms.Animation;
global using Color = Xamarin.Forms.Color;
global using KnownColor = Xamarin.Forms.Color;
global using Point = Xamarin.Forms.Point;
global using Rectangle = Xamarin.Forms.Rectangle;
#else

#endif
