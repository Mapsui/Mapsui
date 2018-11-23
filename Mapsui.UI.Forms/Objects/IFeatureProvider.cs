using Mapsui.Providers;
using System;
using System.ComponentModel;

namespace Mapsui.UI.Objects
{
    public interface IFeatureProvider
    {
        Feature Feature { get; }
//        bool IsVisible { get; }

        //event EventHandler<PropertyChangedEventArgs> PropertyChanged;
    }
}
