using System;

namespace Mapsui.UI;

public interface IMapControlEdit : IMapControl
{
    bool ShiftPressed { get; }
    event Action<object, EditMouseArgs> EditMouseLeftButtonDown;
    event Action<object, EditMouseArgs> EditMouseLeftButtonUp;
    event Action<object, EditMouseArgs> EditMouseMove;
}
