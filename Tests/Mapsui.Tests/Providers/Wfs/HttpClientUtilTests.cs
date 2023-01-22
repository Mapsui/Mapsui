using System.Threading.Tasks;
using Mapsui.Providers.Wfs.Utilities;
using NUnit.Framework;

namespace Mapsui.Tests.Providers.Wfs;

[TestFixture]
public class HttpClientUtilTests
{
    [Test]
    public async Task CloseDoesNotThrowExceptionAsync()
    {
        // Arrange
        using var httpClientUtil = new HttpClientUtil
        {
            Url = "https://www.google.com"
        };

        // Act
        await using var stream = await httpClientUtil.GetDataStreamAsync();

        // Assert
        Assert.DoesNotThrow(httpClientUtil.Close);
    }

    [Test]
    public async Task TwoCloseDoesNotThrowExceptionAsync()
    {
        // Arrange
        using var httpClientUtil = new HttpClientUtil
        {
            Url = "https://www.google.com"
        };

        // Act
        await using var stream = await httpClientUtil.GetDataStreamAsync();
        httpClientUtil.Close();

        // Assert
        Assert.DoesNotThrow(httpClientUtil.Close);
    }
}
