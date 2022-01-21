﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Layers.Tiling;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;
using Newtonsoft.Json;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Mapsui.Samples.Common.Maps
{
    public class AsyncFetchSample : ISample
    {
        public string Name => "Async fetch";
        public string Category => "Special";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateAsyncLayer());
            map.Home = n => n.NavigateTo(map.Layers[1].Extent!.Centroid, map.Resolutions[5]);

            return map;
        }

        private static Layer CreateAsyncLayer()
        {
            // The Layer type fetches data asynchronous (it should be renamed). 
            // This impacts the speed with which data is shown after panning. 
            // This Delayer class is used to filter a continuous stream of 
            // RefreshData requests.
            return new Layer
            {
                Name = "Async Layer",
                IsMapInfoLayer = true,
                DataSource = new MemoryProvider<IFeature>(GetCitiesFromEmbeddedResource()),
                Style = CreateBitmapStyle()
            };
        }

        private static IEnumerable<IFeature> GetCitiesFromEmbeddedResource()
        {
            var path = "Mapsui.Samples.Common.EmbeddedResources.congo.json";
            var assembly = typeof(PointsSample).GetTypeInfo().Assembly;
            using var stream = assembly.GetManifestResourceStream(path);
            var cities = DeserializeFromStream<City>(stream);

            return cities.Select(c => {
                var feature = new PointFeature(SphericalMercator.FromLonLat(c.Lng, c.Lat).ToMPoint());
                feature["name"] = c.Name;
                feature["country"] = c.Country;
                return feature;
            });
        }

        private class City
        {
            public string? Country { get; set; }
            public string? Name { get; set; }
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        public static IEnumerable<T> DeserializeFromStream<T>(Stream stream)
        {
            var serializer = new JsonSerializer();

            using (var sr = new System.IO.StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<List<T>>(jsonTextReader) ?? new List<T>();
            }
        }

        private static SymbolStyle CreateBitmapStyle()
        {
            // For this sample we get the bitmap from an embedded resouce
            // but you could get the data stream from the web or anywhere
            // else.
            var bitmapId = typeof(AsyncFetchSample).LoadBitmapId(@"Images.home.png"); // Designed by Freepik http://www.freepik.com
            var bitmapHeight = 176; // To set the offset correct we need to know the bitmap height
            return new SymbolStyle { BitmapId = bitmapId, SymbolScale = 0.20, SymbolOffset = new Offset(0, bitmapHeight * 0.5) };
        }
    }
}