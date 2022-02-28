// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Mapsui.Rendering.Skia.Tests.Extensions;
using Mapsui.Samples.Common;
using NUnit.Framework;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture, Apartment(ApartmentState.STA)]
public class MapRegressionTests
{
    [Test]
    public void TestAllSamples()
    {
        var exceptions = new List<Exception>();

        foreach (var sample in AllSamples.GetSamples())
        {
            try
            {
                TestSample(sample);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                exceptions.Add(e);
            }
        }

        Assert.AreEqual(exceptions.Count, 0, "No exceptions should happen");
    }

    private void TestSample(ISample sample)
    {
        var fileName = sample.GetType().Name + ".Regression.png";
        var mapControl = new TestMapControl();
        sample.Setup(mapControl);
        var map = mapControl.Map;
        if (map != null)
        {
            var viewport = map.Extent!.Multiply(3).ToViewport(200);

            // act
            using var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(MapRendererTests.CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
        }
    }
}