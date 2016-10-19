using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.Tests.Common.Maps
{
    class EmptySample
    {
        public static Map CreateMap()
        {
            var map = new Map { Viewport = { Center = new Point(0, 0), Width = 200, Height = 200, Resolution = 1 } };
            return map;
        }
    }
}
