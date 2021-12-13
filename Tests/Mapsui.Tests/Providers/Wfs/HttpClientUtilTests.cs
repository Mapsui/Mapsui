using Mapsui.Providers.Wfs.Utilities;
using NUnit.Framework;

namespace Mapsui.Tests.Providers.Wfs
{
    [TestFixture]
    public class HttpClientUtilTests
    {
        [Test]
        public void CloseDoesNotThrowException()
        {
            // Arrange
            using var httpClientUtil = new HttpClientUtil
            {
                Url = "https://www.google.com"
            };

            // Act
            using var stream = httpClientUtil.GetDataStream();

            // Assert
            Assert.DoesNotThrow(httpClientUtil.Close);
        }

        [Test]
        public void TwoCloseDoesNotThrowException()
        {
            // Arrange
            using var httpClientUtil = new HttpClientUtil
            {
                Url = "https://www.google.com"
            };

            // Act
            using var stream = httpClientUtil.GetDataStream();
            httpClientUtil.Close();

            // Assert
            Assert.DoesNotThrow(httpClientUtil.Close);
        }
    }
}
