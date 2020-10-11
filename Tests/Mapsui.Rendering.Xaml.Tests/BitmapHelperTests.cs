using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Mapsui.Rendering.Xaml.Tests
{
    [NUnit.Framework.TestFixtureAttribute, NUnit.Framework.ApartmentAttribute(System.Threading.ApartmentState.STA)]
    public class BitmapHelperTests
    {
        [Test]
        public void LoadSvgTest()
        {
            // arrange
            const string fileName = "Pin.svg";

            // act
            var bitmap = BitmapHelper

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
        }

        [Test]
        public void LoadXmlSvgTest()
        {

        }
    }
}
