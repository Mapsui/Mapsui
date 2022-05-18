using System;
using System.Collections.Generic;
using System.Text;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Tests.Common.TestTools;
using NUnit.Framework;

namespace Mapsui.Tests.Layers
{
    [TestFixture]
    public class ILayerSourceTests
    {
        [Test]
        public void TestTypes()
        {
            var memoryLayer = new TestLayer() { DataSource = new MemoryProvider<IFeature>() };

            if (memoryLayer is ILayerDataSource<IProvider<IFeature>> source)
            {
                Assert.IsTrue(true, "should be true");
            }
            else
            {
                Assert.Fail("We have a problem");
            }
        }
    }
}
