using System.IO;
using Android.App;
using Android.OS;
using Java.IO;
using Java.Lang;
using NUnit.Framework;
using Console = System.Console;
using Exception = System.Exception;
using Android.Graphics;
using Path = System.IO.Path;

namespace Mapsui.Rendering.Android.Tests
{
    [TestFixture]
    public class TestsSample
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void Tear()
        {
        }

        [Test]
        public void Pass()
        {
            Console.WriteLine("test1");

            WriteToFile(CreateBitmap());

            Assert.True(true);
        }


        public static Bitmap CreateBitmap()
        {
            var bitmap = Bitmap.CreateBitmap(200, 200, Bitmap.Config.Argb4444);
            var canvas = new Canvas(bitmap);
            canvas.DrawLine(0, 0, 100, 100, new Paint { Color = new Color(255, 0, 0) });
            return bitmap;

        }

        public static void WriteToFile(Bitmap bitmap)
        {
            try
            {
                var outFile = new MemoryStream();
                bitmap.Compress(Bitmap.CompressFormat.Png, 90, outFile);
                outFile.Position = 0;
                var path = Path.Combine("/sdcard/Download", "test.png");
                var otherPath = Path.Combine(Application.Context.GetExternalFilesDir("unit_test_images").AbsolutePath, "test.png");
                var fos = new FileStream(otherPath, FileMode.OpenOrCreate);
                fos.Write(outFile.ToArray(), 0, (int)outFile.Length);
                fos.Flush();
                fos.Close();
            }
            catch (Exception)
            {
                Console.WriteLine("fout!");
            }

        }

        [Test]
        public void Fail()
        {
            Assert.False(true);
        }

        [Test]
        [Ignore("another time")]
        public void Ignore()
        {
            Assert.True(false);
        }

        [Test]
        public void Inconclusive()
        {
            Assert.Inconclusive("Inconclusive");
        }
    }
}

