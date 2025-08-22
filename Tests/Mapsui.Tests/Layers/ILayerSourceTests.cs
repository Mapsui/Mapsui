using Mapsui.Layers;
using Mapsui.Providers;
using NUnit.Framework;
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
            Assert.That(true, Is.True, "should be true");
        }
        else
        {
            Assert.Fail("We have a problem");
        }
    }
}
