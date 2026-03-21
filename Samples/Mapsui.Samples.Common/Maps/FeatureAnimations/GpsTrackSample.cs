using Mapsui.Animations;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Layers.AnimatedLayers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.FeatureAnimations;

public class GpsTrackSample : ISample
{
    public string Name => "GPS Track";
    public string Category => "FeatureAnimations";

    public Task<Map> CreateMapAsync()
    {
        // How this sample works:
        //
        //   GpsSimulatorProvider  — reads a recorded GPS track from a CSV file and replays it
        //                           one position per second, acting as a stand-in for a real
        //                           GPS device.  It implements IProvider so any layer can use it.
        //
        //   AnimatingProvider     — wraps any IProvider and upgrades its point features into
        //                           AnimatedPointFeature instances.  Each second the underlying
        //                           provider returns a new position; AnimatingProvider smoothly
        //                           interpolates the feature between the old and new location.
        //
        //   Layer                 — a standard layer.  Because its cache contains IAnimatedFeature
        //                           instances, Layer.UpdateAnimations() automatically drives the
        //                           per-frame interpolation without any specialised layer type.

        var simulator = new GpsSimulatorProvider();
        var animatingProvider = new AnimatingProvider(simulator, animationDuration: 1000);

        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer("GpsTrackSampleUserAgent"));
        map.Layers.Add(new Layer("GPS Track")
        {
            DataSource = animatingProvider,
            Style = new ThemeStyle(f => new ImageStyle
            {
                Image = "embedded://Mapsui.Samples.Common.Images.arrow.svg",
                SymbolScale = 0.5,
                SymbolRotation = (double)(f["heading"] ?? 0.0) + 180 // The arrow SVG points down; add 180° to make it point in the direction of travel
            })
        });

        map.CRS = "EPSG:3857";
        map.Navigator.CenterOnAndZoomTo(simulator.StartPosition, map.Navigator.Resolutions[19]);

        // Smoothly trail the vehicle with a 5-second linear pan so the map never jumps.
        simulator.PositionChanged += (_, pos) => map.Navigator.CenterOn(pos, 5000, Easing.Linear);

        return Task.FromResult(map);
    }

    /// <summary>
    /// Wraps any <see cref="IProvider"/> and promotes its point features into
    /// <see cref="AnimatedPointFeature"/> instances so they smoothly interpolate
    /// between position updates.
    /// <para>
    /// Each time the inner provider returns a new position for a feature (matched
    /// by <see cref="Matches"/>), the corresponding <see cref="AnimatedPointFeature"/>
    /// is given the new location as its animation target.  The <see cref="Layer"/>
    /// then drives the per-frame interpolation automatically because it detects
    /// <see cref="IAnimatedFeature"/> instances in its cache.
    /// </para>
    /// </summary>
    internal class AnimatingProvider : IDynamicProvider, IDisposable
    {
        private readonly IProvider _innerProvider;
        private readonly int _animationDuration;
        private readonly Easing _easing;
        private readonly double _distanceThreshold;
        private readonly List<AnimatedPointFeature> _features = [];

        public event EventHandler? DataChanged;

        public AnimatingProvider(IProvider innerProvider, int animationDuration = 1000, Easing? easing = null,
            double distanceThreshold = double.MaxValue)
        {
            _innerProvider = innerProvider;
            _animationDuration = animationDuration;
            _easing = easing ?? Easing.CubicOut;
            _distanceThreshold = distanceThreshold;
            if (innerProvider is IDynamic dynamic)
                dynamic.DataChanged += OnInnerProviderDataChanged;
        }

        private void OnInnerProviderDataChanged(object? sender, EventArgs e)
            => DataChanged?.Invoke(this, e);

        void IDynamic.DataHasChanged() => DataChanged?.Invoke(this, EventArgs.Empty);

        public string? CRS { get => _innerProvider.CRS; set => _innerProvider.CRS = value; }

        public MRect? GetExtent() => _innerProvider.GetExtent();

        /// <summary>
        /// Returns true when <paramref name="existing"/> and <paramref name="incoming"/> represent
        /// the same entity and the existing animated feature should be updated with the new position.
        /// The default implementation compares the "ID" field. Override to use a different key.
        /// </summary>
        protected virtual bool Matches(IFeature existing, IFeature incoming)
            => existing["ID"] is { } id && id.Equals(incoming["ID"]);

        public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
        {
            var incoming = await _innerProvider.GetFeaturesAsync(fetchInfo);
            foreach (var feature in incoming.OfType<PointFeature>())
            {
                var existing = _features.FirstOrDefault(f => Matches(f, feature));
                if (existing is null)
                {
                    var animated = new AnimatedPointFeature(feature.Point.X, feature.Point.Y,
                        _animationDuration, _easing, _distanceThreshold);
                    CopyFieldsAndStyles(feature, animated);
                    _features.Add(animated);
                }
                else
                {
                    existing.SetAnimationTarget(feature.Point);
                    CopyFieldsAndStyles(feature, existing);
                }
            }
            return _features;
        }

        private static void CopyFieldsAndStyles(PointFeature source, AnimatedPointFeature target)
        {
            foreach (var field in source.Fields)
                target[field] = source[field];
            target.Styles = source.Styles.ToList();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_innerProvider is IDynamic dynamic)
                    dynamic.DataChanged -= OnInnerProviderDataChanged;
                if (_innerProvider is IDisposable disposable)
                    disposable.Dispose();
            }
        }
    }

    /// <summary>
    /// Simulates a GPS device by replaying a pre-recorded track from a CSV file.
    /// Every second it advances to the next position that is at least
    /// <see cref="MinMetresPerStep"/> metres away, mimicking realistic movement speed.
    /// <para>
    /// Implements <see cref="IProvider"/> so it can be used as a data source for any
    /// layer or wrapped by <see cref="AnimatingProvider"/> to add smooth animation.
    /// Fires <see cref="IDynamic.DataChanged"/> each tick so the layer knows to
    /// re-fetch, and fires <see cref="PositionChanged"/> so the map can follow the vehicle.
    /// </para>
    /// </summary>
    internal sealed class GpsSimulatorProvider : IDynamicProvider, IDisposable
    {
        public event EventHandler? DataChanged;
        public event EventHandler<MPoint>? PositionChanged;

        private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(1));
        private readonly List<(double Lon, double Lat, double Heading)> _track;
        private int _currentIndex = 500;

        public string? CRS { get; set; }

        /// <summary>Returns the starting map position (used to initialise the viewport).</summary>
        public MPoint StartPosition
        {
            get
            {
                var (lon, lat, _) = _track[_currentIndex];
                return SphericalMercator.FromLonLat(lon, lat).ToMPoint();
            }
        }

        public GpsSimulatorProvider()
        {
            _track = LoadTrack();
            Catch.TaskRun(RunTimerAsync);
        }

        /// <summary>
        /// Not used — the simulator always returns the single current position regardless
        /// of the requested extent, so no meaningful bounding box is available.
        /// </summary>
        public MRect? GetExtent() => null;

        private static List<(double Lon, double Lat, double Heading)> LoadTrack()
        {
            using var stream = typeof(GpsSimulatorProvider).Assembly
                .GetManifestResourceStream("Mapsui.Samples.Common.GeoData.GpsTracks.gps_luckydaxiang.csv")
                ?? throw new InvalidOperationException("GPS track resource not found");
            using var reader = new StreamReader(stream);

            var result = new List<(double Lon, double Lat, double Heading)>();
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                if (!TryParseLine(line, out var entry)) continue;
                // Skip consecutive duplicate positions so every emitted point represents real movement
                if (result.Count > 0)
                {
                    var prev = result[^1];
                    if (Math.Abs(entry.Lon - prev.Lon) < 1e-6 && Math.Abs(entry.Lat - prev.Lat) < 1e-6)
                        continue;
                }
                result.Add(entry);
            }
            return result;
        }

        private static bool TryParseLine(string line, out (double Lon, double Lat, double Heading) entry)
        {
            entry = default;
            var parts = line.Split(',');
            if (parts.Length < 4) return false;
            if (!TryParseDm(parts[0], out var lon) || !TryParseDm(parts[1], out var lat)) return false;
            if (!double.TryParse(parts[3].Trim(), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var heading)) return false;
            entry = (lon, lat, heading);
            return true;
        }

        private static bool TryParseDm(string value, out double degrees)
        {
            // Parses degrees-minutes format: "112°20.4314340′ E"
            degrees = 0;
            var s = value.Trim();
            var degIdx = s.IndexOf('°');
            if (degIdx < 0) return false;
            if (!int.TryParse(s[..degIdx].Trim(), out var d)) return false;
            var afterDeg = s[(degIdx + 1)..].Trim();
            var minEnd = 0;
            while (minEnd < afterDeg.Length && (char.IsDigit(afterDeg[minEnd]) || afterDeg[minEnd] == '.'))
                minEnd++;
            if (!double.TryParse(afterDeg[..minEnd], System.Globalization.CultureInfo.InvariantCulture, out var m)) return false;
            degrees = d + m / 60.0;
            if (s.Contains('W') || s.Contains('S')) degrees = -degrees;
            return true;
        }

        // Advance at least this many metres per timer tick so the vehicle appears to move
        // at a realistic speed. Corresponds roughly to 1 second of travel at ~15 km/h.
        private const double MinMetresPerStep = 5.0;

        private async Task RunTimerAsync()
        {
            while (await _timer.WaitForNextTickAsync())
            {
                AdvanceOneStep();
                var (lon, lat, _) = _track[_currentIndex];
                var pos = SphericalMercator.FromLonLat(lon, lat).ToMPoint();
                DataChanged?.Invoke(this, EventArgs.Empty);
                PositionChanged?.Invoke(this, pos);
            }
        }

        void IDynamic.DataHasChanged() => DataChanged?.Invoke(this, EventArgs.Empty);

        private void AdvanceOneStep()
        {
            var (startLon, startLat, _) = _track[_currentIndex];
            var start = SphericalMercator.FromLonLat(startLon, startLat).ToMPoint();
            var startIndex = _currentIndex;
            do
            {
                _currentIndex = (_currentIndex + 1) % _track.Count;
                var (lon, lat, _) = _track[_currentIndex];
                var current = SphericalMercator.FromLonLat(lon, lat).ToMPoint();
                if (start.Distance(current) >= MinMetresPerStep) return;
            } while (_currentIndex != startIndex); // safety: full loop = no point far enough
        }

        /// <summary>
        /// Returns the vehicle's current position as a single <see cref="PointFeature"/>.
        /// The "ID" field lets <see cref="AnimatingProvider"/> match this feature to the
        /// same animated feature on the next update.  The "heading" field is read by the
        /// layer's <see cref="Mapsui.Styles.Thematics.ThemeStyle"/> to rotate the arrow symbol.
        /// </summary>
        public Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
        {
            var (lon, lat, heading) = _track[_currentIndex];
            var feature = new PointFeature(SphericalMercator.FromLonLat(lon, lat).ToMPoint());
            feature["ID"] = "gps";
            feature["heading"] = heading;
            return Task.FromResult<IEnumerable<IFeature>>([feature]);
        }

        public void Dispose() => _timer.Dispose();
    }
}
