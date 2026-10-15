// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Mapsui.Samples.Common;
using Mapsui.UI.Maui;

namespace Mapsui.Samples.Maui.Model;

internal sealed class SampleScene : IDisposable
{
    private readonly ISampleBase _sample;
    public MapControl Control { get; }
    public bool IsSameSample(ISampleBase selected) => _sample.GetType() == selected.GetType();

    private SampleScene(ISampleBase sample, RenderBackend backend)
    {
        _sample = sample;
        Control = new MapControl(backend);
        Control.Map.Navigator.RotationLock = false;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "The sample source generator constructs every registered sample directly, preserving its public parameterless constructor.")]
    [SuppressMessage("Usage", "IDISP007:Don't dispose injected", Justification = "This scene owns its newly constructed control and the default map being replaced.")]
    public static async Task<SampleScene> CreateAsync(ISampleBase selected, RenderBackend backend, Viewport? viewport, CancellationToken cancellationToken)
    {
        // Registry entries are descriptions, never reused as the lifetime owner of a displayed scene.
        var sample = (ISampleBase?)Activator.CreateInstance(selected.GetType())
            ?? throw new InvalidOperationException($"Could not create sample {selected.Name}.");
        SampleScene? scene = null;
        try
        {
            scene = new SampleScene(sample, backend);
            if (sample is ISample mapSample)
            {
                var map = await mapSample.CreateMapAsync();
                var initialMap = scene.Control.Map;
                scene.Control.Map = map;
                initialMap.Dispose();
            }
            else if (sample is IMapControlSample controlSample)
            {
                var initialMap = scene.Control.Map;
                controlSample.Setup(scene.Control);
                if (!ReferenceEquals(initialMap, scene.Control.Map))
                    initialMap.Dispose();
            }
            cancellationToken.ThrowIfCancellationRequested();

            scene.Control.Map.Navigator.RotationLock = false;
            if (viewport is { Resolution: > 0 } saved)
            {
                // Queue camera restoration until the new native view has its own dimensions.
                scene.Control.Map.Navigator.CenterOnAndZoomTo(new MPoint(saved.CenterX, saved.CenterY), saved.Resolution, 0);
                scene.Control.Map.Navigator.RotateTo(saved.Rotation, 0);
            }
            return scene;
        }
        catch
        {
            if (scene != null)
                scene.Dispose();
            else
                (sample as IDisposable)?.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        Control.Dispose();
        (_sample as IDisposable)?.Dispose();
    }
}
