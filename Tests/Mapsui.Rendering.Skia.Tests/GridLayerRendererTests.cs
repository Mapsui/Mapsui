using NUnit.Framework;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture]
public class GridLayerRendererTests
{
    [TestCase(1_000_000, 6, 200_000)]   // 1 000 000 / 6 = 166 667 → fraction ≈ 1.67 → 2 × 100 000
    [TestCase(1_000_000, 5, 200_000)]   // 1 000 000 / 5 = 200 000 → fraction = 2 → 2 × 100 000
    [TestCase(600, 6, 100)]             // 600 / 6 = 100 → fraction = 1 → 1 × 100
    [TestCase(1200, 6, 200)]            // 1200 / 6 = 200 → fraction = 2 → 2 × 100
    [TestCase(3000, 6, 500)]            // 3000 / 6 = 500 → fraction = 5 → 5 × 100
    [TestCase(8000, 6, 1000)]           // 8000 / 6 ≈ 1333 → magnitude=1000, fraction=1.33 → 1 × 1000
    [TestCase(0.3, 6, 0.05)]            // 0.3 / 6 = 0.05 → magnitude=0.01, fraction=5 → 5 × 0.01
    [TestCase(0.06, 6, 0.01)]           // 0.06 / 6 = 0.01 → magnitude=0.01, fraction=1 → 1 × 0.01
    public void CalcNiceStep_ReturnsExpectedStep(double worldSpan, int targetLineCount, double expectedStep)
    {
        var step = GridLayerRenderer.CalcNiceStep(worldSpan, targetLineCount);

        Assert.That(step, Is.EqualTo(expectedStep).Within(1e-10));
    }

    [Test]
    public void CalcNiceStep_ZeroSpan_ReturnsOne()
    {
        var step = GridLayerRenderer.CalcNiceStep(0, 6);

        Assert.That(step, Is.EqualTo(1));
    }

    [Test]
    public void CalcNiceStep_ZeroTargetCount_ReturnsWorldSpan()
    {
        var step = GridLayerRenderer.CalcNiceStep(1000, 0);

        Assert.That(step, Is.EqualTo(1000));
    }
}
