// Copyright (c) BruTile developers team. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using BruTile;
using BruTile.Cache;
using BruTile.Web;

namespace Mapsui.Utilities
{
    [Obsolete("workaround for user agent issue")]
    public class CustomHttpTileSource : ITileSource, IRequest
    {
        private readonly CustomHttpTileProvider _provider;

        public CustomHttpTileSource(ITileSchema tileSchema, string urlFormatter, IEnumerable<string> serverNodes = null,
            string apiKey = null, string name = null, IPersistentCache<byte[]> persistentCache = null,
            Func<Uri, byte[]> tileFetcher = null, Attribution attribution = null)
            : this(tileSchema, new BasicRequest(urlFormatter, serverNodes, apiKey), name, persistentCache, tileFetcher, attribution)
        {
        }

        public CustomHttpTileSource(ITileSchema tileSchema, IRequest request, string name = null,
            IPersistentCache<byte[]> persistentCache = null, Func<Uri, byte[]> tileFetcher = null, Attribution attibution = null)
        {
            _provider = new CustomHttpTileProvider(request, persistentCache, tileFetcher);
            Schema = tileSchema;
            Name = name ?? string.Empty;
            Attribution = attibution ?? new Attribution();
        }

        public IPersistentCache<byte[]> PersistentCache => _provider.PersistentCache;

        public Uri GetUri(TileInfo tileInfo)
        {
            return _provider.GetUri(tileInfo);
        }

        public ITileSchema Schema { get; }

        public string Name { get; set; }

        public Attribution Attribution { get; set; }

        /// <summary>
        /// Gets the actual image content of the tile as byte array
        /// </summary>
        public virtual byte[] GetTile(TileInfo tileInfo)
        {
            return _provider.GetTile(tileInfo);
        }
    }
}