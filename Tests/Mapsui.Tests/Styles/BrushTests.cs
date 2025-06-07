using Mapsui.Styles;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Mapsui.Tests.Styles;

[TestFixture]
internal class BrushTests
{
    [Test]
    public void DefaultConstructor_SetsPropertiesToNullOrDefaults()
    {
        var brush = new Brush();

        Assert.That(brush.Color, Is.Null);
        Assert.That(brush.Background, Is.Null);
        Assert.That(brush.Image, Is.Null);
        Assert.That(brush.FillStyle, Is.EqualTo(FillStyle.Solid));
    }

    [Test]
    public void ColorConstructor_SetsColor()
    {
        var brush = new Brush(Color.Red);

        Assert.That(brush.Color, Is.EqualTo(Color.Red));
        Assert.That(brush.FillStyle, Is.EqualTo(FillStyle.Solid));
    }

    [Test]
    public void CopyConstructor_CopiesAllPropertiesExceptImage()
    {
        var original = new Brush
        {
            Color = Color.Blue,
            Background = Color.Green,
            FillStyle = FillStyle.Bitmap
        };
        var copy = new Brush(original);

        Assert.That(copy.Color, Is.EqualTo(original.Color));
        Assert.That(copy.Background, Is.EqualTo(original.Background));
        Assert.That(copy.FillStyle, Is.EqualTo(original.FillStyle));
        Assert.That(copy.Image, Is.Null); // Image is not copied by the copy constructor
    }

    [Test]
    public void SettingImage_ChangesFillStyleToBitmap()
    {
        var brush = new Brush { FillStyle = FillStyle.Solid };
        var image = new Image { Source = "file://test.png" };

        brush.Image = image;

        Assert.That(brush.Image, Is.SameAs(image));
        Assert.That(brush.FillStyle, Is.EqualTo(FillStyle.Bitmap));
    }

    [Test]
    public void SettingImage_DoesNotChangeFillStyleIfAlreadyBitmapOrBitmapRotated()
    {
        var brush = new Brush { FillStyle = FillStyle.BitmapRotated };
        var image = new Image { Source = "file://test.png" };

        brush.Image = image;

        Assert.That(brush.FillStyle, Is.EqualTo(FillStyle.BitmapRotated));
    }

    [Test]
    public void Equals_ReturnsTrue_ForIdenticalBrushes()
    {
        var image = new Image { Source = "file://test.png" };
        var brush1 = new Brush
        {
            Color = Color.Red,
            Background = Color.Blue,
            FillStyle = FillStyle.Bitmap,
            Image = image
        };
        var brush2 = new Brush
        {
            Color = Color.Red,
            Background = Color.Blue,
            FillStyle = FillStyle.Bitmap,
            Image = image
        };

        Assert.That(brush1, Is.EqualTo(brush2));
        Assert.That(brush1 == brush2, Is.True);
        Assert.That(brush1 != brush2, Is.False);
    }

    [Test]
    public void Equals_ReturnsFalse_ForDifferentBrushes()
    {
        var brush1 = new Brush { Color = Color.Red };
        var brush2 = new Brush { Color = Color.Blue };

        Assert.That(brush1, Is.Not.EqualTo(brush2));
        Assert.That(brush1 == brush2, Is.False);
        Assert.That(brush1 != brush2, Is.True);
    }

    [Test]
    public void GetHashCode_IsEqual_ForIdenticalBrushes()
    {
        var image = new Image { Source = "file://test.png" };
        var brush1 = new Brush
        {
            Color = Color.Red,
            Background = Color.Blue,
            FillStyle = FillStyle.Bitmap,
            Image = image
        };
        var brush2 = new Brush
        {
            Color = Color.Red,
            Background = Color.Blue,
            FillStyle = FillStyle.Bitmap,
            Image = image
        };

        Assert.That(brush1.GetHashCode(), Is.EqualTo(brush2.GetHashCode()));
    }
    [Test]
    public void Brush_MutatingAfterDictionaryInsert_BreaksLookup()
    {
        var brush = new Brush
        {
            Color = Color.Red,
            Background = Color.Blue,
            FillStyle = FillStyle.Bitmap
        };

        var dict = new Dictionary<Brush, string>
        {
            [brush] = "test-value"
        };

        // Mutate a property that affects hash code and equality
        brush.Color = Color.Green;

        // Now the dictionary cannot find the brush anymore
        Assert.That(dict.ContainsKey(brush), Is.False, "Mutating a key after insertion breaks dictionary lookup.");
    }

    [Test]
    public void Brush_MutatingManyAfterDictionaryInsert_BreaksLookupOften()
    {
        var rand = new Random(42);
        var dict = new Dictionary<Brush, string>();
        var brushes = new List<Brush>();
        int count = 1000000;
        int failedLookups = 0;

        // Insert random brushes
        for (int i = 0; i < count; i++)
        {
            var brush = new Brush
            {
                Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256), 255),
                Background = new Color(rand.Next(256), rand.Next(256), rand.Next(256), 255),
                FillStyle = (FillStyle)(rand.Next(Enum.GetValues(typeof(FillStyle)).Length))
            };
            dict[brush] = $"value-{i}";
            brushes.Add(brush);
        }

        // Mutate each brush after insertion
        foreach (var brush in brushes)
        {
            brush.Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256), 255);
            brush.Background = new Color(rand.Next(256), rand.Next(256), rand.Next(256), 255);
            brush.FillStyle = (FillStyle)(rand.Next(Enum.GetValues(typeof(FillStyle)).Length));
        }

        // Check how many mutated brushes can still be found
        foreach (var brush in brushes)
        {
            if (!dict.ContainsKey(brush))
                failedLookups++;
        }

        // It is very likely that at least some lookups will fail
        Assert.That(failedLookups, Is.GreaterThan(0),
            $"Expected at least some failed lookups after mutation, but all {count} succeeded.");
    }

}
