using System.Collections.Generic;
using Mapsui.Experimental.VectorTiles.VexTileCopies;
using NUnit.Framework;
using VexTile.Renderer.Mvt.AliFlux.Drawing;

namespace Mapsui.Experimental.VectorTiles.Tests.VexTileCopies;

[TestFixture]
public class LineClipperTests
{
    [Test]
    public void ClipPolyline_LineLeavesAndReenters_ReportsTwoPieces()
    {
        // Inside, out over the right edge, back in: two pieces, not one line with a chord.
        var line = new List<Point> { new(10, 10), new(90, 10), new(150, 50), new(90, 90), new(10, 90) };
        var result = new List<Point>();
        var partStarts = new List<int>();

        var clipped = LineClipper.ClipPolyline(line, new Rect(0, 0, 100, 100), result, partStarts);

        Assert.That(clipped, Is.True);
        Assert.That(partStarts, Has.Count.EqualTo(1));
        Assert.That(result[partStarts[0] - 1].X, Is.EqualTo(100).Within(1e-9));
        Assert.That(result[partStarts[0]].X, Is.EqualTo(100).Within(1e-9));
        Assert.That(result[partStarts[0]].Y, Is.GreaterThan(result[partStarts[0] - 1].Y));
    }

    [Test]
    public void ClipPolyline_LineInside_IsOnePiece()
    {
        var line = new List<Point> { new(10, 10), new(50, 50), new(90, 10) };
        var result = new List<Point>();
        var partStarts = new List<int>();

        LineClipper.ClipPolyline(line, new Rect(0, 0, 100, 100), result, partStarts);

        Assert.That(partStarts, Is.Empty);
        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public void LongestPart_TwoPieces_ReturnsTheLongerOne()
    {
        var points = new List<Point> { new(0, 0), new(10, 0), new(0, 50), new(80, 50) };

        var part = SkiaCanvas.LongestPart(points, new List<int> { 2 });

        Assert.That(part, Has.Count.EqualTo(2));
        Assert.That(part[1].X, Is.EqualTo(80));
    }
}
