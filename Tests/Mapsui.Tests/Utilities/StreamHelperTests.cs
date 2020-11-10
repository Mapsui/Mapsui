﻿using Mapsui.Extensions;
using NUnit.Framework;
using Mapsui.Rendering.Xaml.Tests;

namespace Mapsui.Tests.Utilities
{
    [TestFixture]
    public class StreamHelperTests
    {
        [Test]
        public void StreamIsSvgPin()
        {
            // act
            var stream = File.ReadFromOriginalFolder("Pin.svg");

            // assert
            Assert.True(stream.IsSvg());
        }

        [Test]
        public void StreamIsSvgPinXml()
        {
            // act
            var stream = File.ReadFromOriginalFolder("PinXml.svg");

            // assert
            Assert.True(stream.IsSvg());
        }

        [Test]
        public void StreamIsSvgVectorSymbolUnittype()
        {
            // act
            var stream = File.ReadFromOriginalFolder("vector_symbol_unittype.png");

            // assert
            Assert.IsFalse(stream.IsSvg());
        }
    }
}