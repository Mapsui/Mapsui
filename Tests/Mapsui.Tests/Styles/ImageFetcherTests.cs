using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Styles;
using NUnit.Framework;

namespace Mapsui.Tests.Styles;

[TestFixture]
public static class ImageFetcherTests
{
    // Turkish lowercases 'I' to the dotless 'ı', while Uri.Host always lowercases it to 'i'.
    // See https://github.com/Mapsui/Mapsui/issues/3383
    [Test, SetCulture("tr-TR")]
    public static void EmbeddedResourceIndexMatchesUriHostUnderTurkishCulture()
    {
        // Arrange
        var host = new Uri("embedded://Mapsui.Resources.Images.Pin.svg").Host;

        // Act
        var embeddedResources = ImageFetcher.LoadEmbeddedResourcePaths();

        // Assert
        Assert.That(embeddedResources.ContainsKey(host), Is.True);
    }

    // In Blazor WebAssembly the response stream is backed by the browser's Fetch API and throws on synchronous reads.
    // See https://github.com/Mapsui/Mapsui/issues/3381
    [Test]
    public static async Task LoadFromUrlReadsResponseStreamAsynchronouslyAsync()
    {
        // Arrange
        var expected = new byte[] { 1, 2, 3, 4, 5 };
        using var handler = new AsyncOnlyStreamHandler(expected);

        // Act
        var bytes = await ImageFetcher.LoadFromUrlAsync(new Uri("https://example.com/image.png"), handler);

        // Assert
        Assert.That(bytes, Is.EqualTo(expected));
    }

    private sealed class AsyncOnlyStreamHandler(byte[] content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
#pragma warning disable IDISP004 // StreamContent takes ownership of the stream and disposes it
                Content = new StreamContent(new AsyncOnlyStream(content)),
#pragma warning restore IDISP004
            });
        }
    }

    /// <summary>Mimics the browser's Fetch-backed response stream: async reads only, no seeking.</summary>
    private sealed class AsyncOnlyStream(byte[] content) : Stream
    {
        private readonly MemoryStream _inner = new(content);

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException("Synchronous reads are not supported.");

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            Task.FromResult(_inner.Read(buffer, offset, count));

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(_inner.Read(buffer.Span));

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
