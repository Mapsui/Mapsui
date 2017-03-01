using System;
using System.Collections.Generic;
using System.IO;

namespace Mapsui.Styles
{
    public class BitmapRegistry
    {
        private static BitmapRegistry _instance;
        private readonly IDictionary<int, Stream> _register = new Dictionary<int, Stream>();
        private BitmapRegistry() {}
        private int _counter;

        public static BitmapRegistry Instance => _instance ?? (_instance = new BitmapRegistry());

        public int Register(Stream bitmapData)
        {
            if (bitmapData == null) throw new ArgumentException(
                "The bitmap data that is registered is null. Was the image loaded correctly?");

            var id = _counter;
            _counter++;
            _register[id] = bitmapData;
            return id;
        }

        public void Unregister(int id)
        {
            _register.Keys.Remove(id);
        }

        public Stream Get(int id)
        {
            return _register[id];
        }
    }
}
