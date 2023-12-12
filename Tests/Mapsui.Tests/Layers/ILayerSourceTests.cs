using System;
using System.Collections.Generic;
using System.Text;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Tests.Common.TestTools;
using Mapsui.Tiling.Layers;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Mapsui.Tests.Layers;

[TestFixture]
public class ILayerSourceTests
{
    [Test]
    public void TestTypes()
    {
        using var memoryLayer = new Layer() { DataSource = new MemoryProvider() };

        if (memoryLayer is ILayerDataSource<IProvider> source)
        {
            ClassicAssert.IsTrue(true, "should be true");
        }
        else
        {
            Assert.Fail("We have a problem");
        }
    }
}
