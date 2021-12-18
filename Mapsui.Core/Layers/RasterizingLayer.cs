using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Logging;
using Mapsui.Rendering;

namespace Mapsui.Layers
{
    public class RasterizingTileLayer : RasterizingLayer
    {
        /// <summary>
        ///     Creates a RasterizingTileLayer which rasterizes a layer for performance
        /// </summary>
        /// <param name="layer">The Layer to be rasterized</param>
        /// <param name="delayBeforeRasterize">Delay after viewport change to start re-rasterizing</param>
        /// <param name="renderResolutionMultiplier"></param>
        /// <param name="rasterizer">Rasterizer to use. null will use the default</param>
        /// <param name="overscanRatio">The ratio of the size of the rasterized output to the current viewport</param>
        /// <param name="onlyRerasterizeIfOutsideOverscan">
        ///     Set the rasterization policy. false will trigger a rasterization on
        ///     every viewport change. true will trigger a re-rasterization only if the viewport moves outside the existing
        ///     rasterization.
        /// </param>
        /// <param name="pixelDensity"></param>
        public RasterizingTileLayer(
            ILayer layer,
            int delayBeforeRasterize = 1000,
            double renderResolutionMultiplier = 1,
            IRenderer? rasterizer = null,
            double overscanRatio = 1,
            bool onlyRerasterizeIfOutsideOverscan = false,
            float pixelDensity = 1) : base(
            layer, 
            delayBeforeRasterize, 
            renderResolutionMultiplier, 
            rasterizer,
            overscanRatio, 
            onlyRerasterizeIfOutsideOverscan, 
            pixelDensity)
        {
        }
    }
}