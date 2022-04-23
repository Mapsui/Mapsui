using System;
using System.Collections.Generic;
using System.Text;
using Mapsui.Layers;
using Mapsui.Providers;
using NUnit.Framework;

namespace Mapsui.Tests.Layers
{
    [TestFixture]
    public class ILayerSourceTests
    {
        [Test]
        public void TestTypes()
        {
            var memoryLayer = new MemoryLayer("test");
            memoryLayer.DataSource = new MemoryProvider<IFeature>();

            if (memoryLayer is ILayerDataSource<IProviderBase> source)
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
