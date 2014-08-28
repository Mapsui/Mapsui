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

        public static BitmapRegistry Instance
        {
            get { return _instance ?? (_instance = new BitmapRegistry()); }
        }

        public int Register(Stream stream)
        {
            var id = _counter;
            _counter++;
            _register[id] = stream;
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
