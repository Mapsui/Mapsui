using System.Collections.Generic;
using Mapsui.Styles;

namespace Mapsui.Rendering.Skia
{
    public class SymbolCache : ISymbolCache
    {
        private readonly IDictionary<int, BitmapInfo> _cache = new Dictionary<int, BitmapInfo>();

        public BitmapInfo GetOrCreate(int bitmapId, bool isSvg = false)
        {
            if (_cache.Keys.Contains(bitmapId)) return _cache[bitmapId];
            return _cache[bitmapId] = BitmapHelper.LoadBitmap(BitmapRegistry.Instance.Get(bitmapId), isSvg);
        }

        public Size GetSize(int bitmapId)
        {
            var bitmap = GetOrCreate(bitmapId);
            return new Size(bitmap.Width, bitmap.Height);
        }
    }
}