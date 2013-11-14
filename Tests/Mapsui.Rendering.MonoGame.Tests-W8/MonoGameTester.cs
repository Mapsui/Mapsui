using Mapsui.Rendering.XamlRendering.Tests;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Mapsui.Rendering.MonoGame.Tests_W8
{
    /// <summary>
    /// This is not a unit test project. I was not able to instantiate the GraphicsDevice
    /// from within the test framework. For now I run this project as a MonoGame Game and
    /// visually inspect the graphics to compare them with the xaml output. This has been
    /// very helpfull but I would like to turn this in a proper unit test at some point.
    /// </summary>
    public class MonoGameTester : Game
    {
        public MonoGameTester()
        {
            new GraphicsDeviceManager(this);
        }

        protected override void Draw(GameTime gameTime)
        {
            new MapRendererTests(GraphicsDevice).RenderPointsWithDifferentSymbolTypes();
            
            base.Draw(gameTime);
            Exit();
        }

        public static async Task<byte[]> ReadFile(string filePath)
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///file.txt"));
            var stream = await file.OpenStreamForReadAsync();
            return Tests.Common.Utilities.ToByteArray(stream);
        }

        public static async Task WriteFile(string path, MemoryStream memoryStream)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;

            //!!!var file = await folder.GetFileAsync(path);
            //!!!if (file != null) await file.DeleteAsync();
            var file = await folder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists);

            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (var outputStream = fileStream.GetOutputStreamAt(0))
                {
                    using (var dataWriter = new DataWriter(outputStream))
                    {
                        //TODO: Replace "Bytes" with the type you want to write.
                        dataWriter.WriteBytes(memoryStream.ToArray());
                        await dataWriter.StoreAsync();
                        dataWriter.DetachStream();
                    }

                    await outputStream.FlushAsync();
                }
            }
        }
    }
}
