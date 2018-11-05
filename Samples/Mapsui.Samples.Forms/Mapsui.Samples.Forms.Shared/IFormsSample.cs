using Mapsui.Samples.Common;
using System;

namespace Mapsui.Samples.Forms
{
    public interface IFormsSample : ISample
    {
        bool OnClick(object sender, EventArgs args);
    }
}
