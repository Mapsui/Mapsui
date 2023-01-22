using Mapsui.Providers.Wfs.Utilities;
using NUnit.Framework;

namespace Mapsui.Tests.Providers.Wfs.Utilities;

[TestFixture]
public class UrlAppendQueryHelperTests
{
    [Test]
    public void AppendQueryToUrl()
    {
        // Arrange
        var url = "www.test.de/test";
        var query = "?query=test";

        // Act
        var combinedUrl = url.AppendQuery(query);

        // Assert
        Assert.AreEqual("www.test.de/test?query=test", combinedUrl);
    }

    [Test]
    public void AppendQueryToQueryUrl()
    {
        // Arrange
        var url = "www.test.de/test?Service=WFS";
        var query = "?query=test";

        // Act
        var combinedUrl = url.AppendQuery(query);

        // Assert
        Assert.AreEqual("www.test.de/test?Service=WFS&query=test", combinedUrl);
    }

    [Test]
    public void AppendQueryToQueryUrlWithAppersend()
    {
        // Arrange
        var url = "www.test.de/test?Service=WFS&";
        var query = "?query=test";

        // Act
        var combinedUrl = url.AppendQuery(query);

        // Assert
        Assert.AreEqual("www.test.de/test?Service=WFS&query=test", combinedUrl);
    }

    [Test]
    public void AppendQueryToQueryUrlWithOutQuestionMark()
    {
        // Arrange
        var url = "www.test.de/test?Service=WFS";
        var query = "query=test";

        // Act
        var combinedUrl = url.AppendQuery(query);

        // Assert
        Assert.AreEqual("www.test.de/test?Service=WFS&query=test", combinedUrl);
    }
}
