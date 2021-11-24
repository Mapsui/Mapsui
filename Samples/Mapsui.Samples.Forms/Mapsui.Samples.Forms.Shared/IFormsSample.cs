using Mapsui.Samples.Common;
using System;

#if __MAUI__
namespace Mapsui.Samples.Maui
#else
namespace Mapsui.Samples.Forms
#endif
{
    public interface IFormsSample : ISample
    {
        bool OnClick(object? sender, EventArgs args);
    }
}
