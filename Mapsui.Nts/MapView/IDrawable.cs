using System;
using System.Collections.Generic;
using System.Text;
using Mapsui.Nts;

// ReSharper disable once CheckNamespace
namespace Mapsui;

public interface IDrawable
{
    GeometryFeature? Feature { get; set; }
    bool IsClickable { get; set; }
    void HandleClicked(IDrawableClicked drawableArgs);
}
