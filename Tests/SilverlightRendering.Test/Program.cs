using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SharpMap.Providers;
using SharpMap.Geometries;
using SharpMap;
using SharpMap.Layers;
using SharpMap.Styles;

namespace SilverlightRendering.Test
{
    [TestFixture]
    class SilverlightRendererTest
    {
        [Test]
        public static void RenderLayer()
        {
            var provider = new MemoryProvider();

            var feature = new Feature();
            feature.Geometry = new Point(50, 50);
            provider.Features.Add(feature);
            
            var renderer = new MapRenderer();
            var view = new View{ Center = new Point(50, 50), Width = 100, Height = 100 };
            var map = new Map();
            map.Layers.Add(new Layer("test") { DataSource = provider});
            renderer.Render(view, map.Layers);
        }
    }
}
