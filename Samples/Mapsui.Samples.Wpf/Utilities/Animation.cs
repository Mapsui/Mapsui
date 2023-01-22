using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Mapsui.Samples.Wpf.Utilities;

public static class Animation
{
    public static void AnimateOpacity(UIElement target, double from, double to, int duration)
    {
        target.Opacity = 0;
        var animation = new DoubleAnimation();
        animation.From = from;
        animation.To = to;
        animation.Duration = new TimeSpan(0, 0, 0, 0, duration);

        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));

        var storyBoard = new Storyboard();
        storyBoard.Children.Add(animation);
        storyBoard.Begin();
    }
}
