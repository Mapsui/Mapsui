using System;
using System.Collections.Generic;
using System.IO;

namespace Mapsui.Styles
{
    public class BitmapRegistry
    {
        private static BitmapRegistry _instance;
        private readonly IDictionary<int, object> _register = new Dictionary<int, object>();
        private BitmapRegistry() {}
        private int _counter;

        public static BitmapRegistry Instance => _instance ?? (_instance = new BitmapRegistry());

        public int Register(object bitmapData)
        {
            if (bitmapData == null) throw new ArgumentException(
                "The bitmap data that is registered is null. Was the image loaded correctly?");

            if (bitmapData is Atlas atlas)
            {
                if (atlas.BitmapId < 0 || !_register.ContainsKey(atlas.BitmapId))
                {
                    throw new ArgumentException("Atlas has no corresponding atlas bitmap.");
                }
            }

            var id = _counter;
            _counter++;
            _register[id] = bitmapData;
            return id;
        }

        public void Unregister(int id)
        {
            _register.Remove(id);
        }

        public object Get(int id)
        {
            return _register[id];
        }
    }
}
