// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Samples.Maui.Model;
using NUnit.Framework;

namespace Mapsui.UI.Maui.Tests;

#pragma warning disable IDISP016, IDISP017 // Deliberately test cancellation and request rejection after disposal.
#pragma warning disable VSTHRD003 // Test-owned completion sources control asynchronous ordering; there are no blocking waits.

[TestFixture]
public class SampleSwitchCoordinatorTests
{
    [Test]
    public async Task RapidSelectionsPublishOnlyTheLatestAsync()
    {
        using var coordinator = new SampleSwitchCoordinator();
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var published = new List<int>();
        var cleanedUp = false;
        var first = coordinator.RunAsync(async token =>
        {
            try
            {
                await release.Task;
                token.ThrowIfCancellationRequested();
                published.Add(1);
            }
            finally { cleanedUp = true; }
        });
        var second = coordinator.RunAsync(_ => { published.Add(2); return Task.CompletedTask; });
        var third = coordinator.RunAsync(_ =>
        {
            Assert.That(cleanedUp, Is.True, "A replacement must wait for the previous scene to finish cleanup.");
            published.Add(3);
            return Task.CompletedTask;
        });
        Assert.That(third.IsCompleted, Is.False);
        release.SetResult();
        await Task.WhenAll(first, second, third);
        Assert.That(published, Is.EqualTo(new[] { 3 }));
    }

    [Test]
    public async Task DisposalCancelsPendingPublicationAsync()
    {
        using var coordinator = new SampleSwitchCoordinator();
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var published = false;
        var pending = coordinator.RunAsync(async token =>
        {
            await release.Task;
            token.ThrowIfCancellationRequested();
            published = true;
        });
        coordinator.Dispose();
        release.SetResult();
        await pending;
        Assert.That(published, Is.False);
        Assert.Throws<ObjectDisposedException>(() => { _ = coordinator.RunAsync(_ => Task.CompletedTask); });
    }

    [Test]
    public async Task FailedSelectionDoesNotPreventTheNextAsync()
    {
        using var coordinator = new SampleSwitchCoordinator();
        var failed = coordinator.RunAsync(_ => Task.FromException(new InvalidOperationException("sample failed")));
        Assert.ThrowsAsync<InvalidOperationException>(async () => await failed);
        var published = false;
        await coordinator.RunAsync(_ => { published = true; return Task.CompletedTask; });
        Assert.That(published, Is.True);
    }
}
