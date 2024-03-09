using Mapsui.Manipulations;
using NUnit.Framework;
using System.Collections.Generic;

namespace Mapsui.Tests.Manipulations;

[TestFixture]
public class ManipulationTrackerTests
{
    record Input(ScreenPosition[] Locations, Manipulation? Manipulation, string Message);

    [Test]
    public void ManipulationSequenceTests()
    {
        // Arrange
        var inputs = new List<Input>
        {
            new([], null, "No touches and no previous input."),
            new([], null, "Still no touches and so no manipulation."),
            new([new(0, 0), new(1, 0)], null, "First touch but no previous input so no manipulation."),
            new([new(0, 0), new(0, 1)], new Manipulation(new ScreenPosition(0, 0.5), new ScreenPosition(0.5, 0), 1, 90, 90), "Rotate 90 degrees."),
            new([new(0, 0), new(0, 2)], new Manipulation(new ScreenPosition(0, 1), new ScreenPosition(0, 0.5), 2, 0, 90), "Move one finger to the outside to scale a factor of 2."),
            new([new(0, 0)], null, "Lift one finger. This is considered a restart because we cannot compare centers, scale or rotation."),
            new([new(1, 1)], new Manipulation(new ScreenPosition(1, 1), new ScreenPosition(0, 0), 1, 0, 0), "Move one finger. The positions change but nothing else."),
            new([], null, "Lift the remaining finger. There is no manipulation."),
            new([new(2, 2)], null, "Put finger down. There is still no manipulation."),
            new([new(2, 2)], null, "Nothing changes. This should be handled as no manipulation."),
            new([new(3, 3)], new Manipulation(new ScreenPosition(3, 3), new ScreenPosition(2, 2), 1, 0, 0), "Ordinary drag operation."),
            new([new(2, 2)], new Manipulation(new ScreenPosition(2, 2), new ScreenPosition(3, 3), 1, 0, 0), "Drag back again."),
            new([new(0, 0), new(4, 4)], null, "No change in TouchState so no manipulation. Internally the angle is now stored which will show in the next update."),
            new([new(0, 4), new(4, 0)], new Manipulation(new ScreenPosition(2, 2), new ScreenPosition(2, 2), 1, -90, -90), "Same center but rotation has changed."),
            new([new(1, 3), new(3, 1)], new Manipulation(new ScreenPosition(2, 2), new ScreenPosition(2, 2), 0.5, 0, -90), "Same center but scale decreases. Total rotation is still preserved."),
            new([new(0, 4), new(4, 0)], new Manipulation(new ScreenPosition(2, 2), new ScreenPosition(2, 2), 2, 0, -90), "Same center but scale increases. Total rotation is still preserved."),
            new([new(4, 4), new(0, 0)], new Manipulation(new ScreenPosition(2, 2), new ScreenPosition(2, 2), 1, -90, -180), "Same center. Total rotation increases."),
            new([new(0, 4), new(4, 0), new (0, 0), new(4, 4)], null, "Same center but reset because the finger count changed."),
        };

        var manipulationTracker = new ManipulationTracker();

        foreach (var input in inputs)
        {
            // Act
            manipulationTracker.Manipulate(input.Locations, (m) =>
            {
                // Assert
                Assert.That(m, Is.EqualTo(input.Manipulation), input.Message);
            });
        }
    }
}
