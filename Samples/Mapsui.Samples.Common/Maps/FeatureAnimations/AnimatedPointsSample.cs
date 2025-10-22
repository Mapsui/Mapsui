﻿using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Layers.AnimatedLayers;
using Mapsui.Providers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using Mapsui.Tiling.Extensions;
using Mapsui.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.FeatureAnimations;

public sealed class AnimatedPointsSample : ISample, IDisposable
{
    private bool _disposed;
    readonly AnimatedPointsSampleProvider _animatedPointsSampleProvider = new();

    public string Name => "AnimatedPoints";
    public string Category => "FeatureAnimations";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateAnimatedPointLayer(_animatedPointsSampleProvider));
        return Task.FromResult(map);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _animatedPointsSampleProvider.Dispose();

        _disposed = true;
    }

    private static AnimatedPointLayer CreateAnimatedPointLayer(AnimatedPointsSampleProvider animatedPointsSampleProvider) => new(animatedPointsSampleProvider)
    {
        Name = "Animated Points",
        Style = CreatePointStyle()
    };

    private static ThemeStyle CreatePointStyle() => new(CreateSvgArrowStyle);

    private static ImageStyle CreateSvgArrowStyle(IFeature feature) => new()
    {
        Image = "embedded://Mapsui.Samples.Common.Images.arrow.svg",
        SymbolScale = 0.5,
        RelativeOffset = new RelativeOffset(0.0, 0.5),
        Opacity = 0.5f,
        SymbolRotation = (double)feature["rotation"]!
    };

    internal class AnimatedPointsSampleProvider : MemoryProvider, IDynamic, IDisposable
    {
        private readonly Timer _timer;
        private readonly Random _random = new(0);
        private List<PointFeature> _previousFeatures = new();
        private List<PointFeature> features = new();
        private static readonly MRect _extent = new GlobalSphericalMercator().Extent.ToMRect();

        public AnimatedPointsSampleProvider()
        {
            _timer = new Timer(_ =>
            {
                DataHasChanged();
                features = CreateNewFeatures(_random, _previousFeatures);
                _previousFeatures = MergeWithPreviousFeatures(_previousFeatures, features);
            }, this, 0, 1600);
        }

        public event EventHandler? DataChanged;

        public override Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
        {
            return Task.FromResult((IEnumerable<IFeature>)features);
        }

        private static List<PointFeature> CreateNewFeatures(Random random, List<PointFeature> previousFeatures)
        {
            var features = new List<PointFeature>();
            var points = RandomPointsBuilder.GenerateRandomPoints(_extent, 10, random).ToList();
            var count = 0;
            var randomItemToSkip = random.Next(points.Count);

            foreach (var point in points)
            {
                count++;
                if (count == randomItemToSkip)
                    continue; // Skip a random element to test robustness.

                var countAsString = count.ToString(CultureInfo.InvariantCulture);
                features.Add(new PointFeature(point)
                {
                    ["ID"] = countAsString,
                    ["rotation"] = (AngleOf(point, FindPreviousPosition(countAsString, previousFeatures), random) - 90 + 360) % 360
                });
            }

            return features;
        }

        private static MPoint? FindPreviousPosition(string countAsString, List<PointFeature> previousFeatures)
        {
            return previousFeatures.FirstOrDefault(f => f["ID"]?.ToString() == countAsString)?.Point;
        }

        public static double AngleOf(MPoint point1, MPoint? point2, Random random)
        {
            if (point2 == null)
                return random.Next(360);
            double result = Algorithms.RadiansToDegrees(Math.Atan2(point1.Y - point2.Y, point2.X - point1.X));
            return (result < 0) ? (360.0 + result) : result;
        }

        public void DataHasChanged()
        {
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, new EventArgs());
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
            }
        }
        private static List<PointFeature> MergeWithPreviousFeatures(List<PointFeature> previousFeatures, List<PointFeature> newFeatures)
        {
            // Some features are missing in the new list (to test robustness). We want to store the missing ones as well.
            return newFeatures
                .Concat(previousFeatures)
                .GroupBy(f => f["ID"])
                .Select(g => g.First())
                .ToList();
        }
    }

}
