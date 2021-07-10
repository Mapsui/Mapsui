using Mapsui.Providers.Wfs.Utilities;
using NUnit.Framework;

namespace Mapsui.Tests.Providers.Wfs.Utilities
{
    [TestFixture]
    public class UrlAppendQueryHelperTests
    {
        [Test]
        public void AppendQueryToUrl()
        {
            // Arrange
            string url = "www.test.de/test";
            string query = "?query=test";

            // Act
            string combinedUrl = url.AppendQuery(query);

            // Assert
            Assert.AreEqual("www.test.de/test?query=test" ,combinedUrl);
        }

        [Test]
        public void AppendQueryToQueryUrl()
        {
            // Arrange
            string url = "www.test.de/test?Service=WFS";
            string query = "?query=test";

            // Act
            string combinedUrl = url.AppendQuery(query);

            // Assert
            Assert.AreEqual("www.test.de/test?Service=WFS&query=test" ,combinedUrl);
        }

        [Test]
        public void AppendQueryToQueryUrlWithAppersend()
        {
            // Arrange
            string url = "www.test.de/test?Service=WFS&";
            string query = "?query=test";

            // Act
            string combinedUrl = url.AppendQuery(query);

            // Assert
            Assert.AreEqual("www.test.de/test?Service=WFS&query=test" ,combinedUrl);
        }

        [Test]
        public void AppendQueryToQueryUrlWithOutQuestionMark()
        {
            // Arrange
            string url = "www.test.de/test?Service=WFS";
            string query = "query=test";

            // Act
            string combinedUrl = url.AppendQuery(query);

            // Assert
            Assert.AreEqual("www.test.de/test?Service=WFS&query=test" ,combinedUrl);
        }
    }
}
