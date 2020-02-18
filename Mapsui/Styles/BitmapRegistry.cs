using System;
using System.Collections.Generic;

namespace Mapsui.Styles
{
    /// <summary>
    /// Class for managing all bitmaps, which are registered for Mapsui drawing
    /// </summary>
    public class BitmapRegistry
    {
        private static BitmapRegistry _instance;
        private readonly IDictionary<int, object> _register = new Dictionary<int, object>();
        private BitmapRegistry() {}
        private int _counter;

        /// <summary>
        /// Singleton of BitmapRegistry class
        /// </summary>
        public static BitmapRegistry Instance => _instance ?? (_instance = new BitmapRegistry());

        /// <summary>
        /// Register a new bitmap
        /// </summary>
        /// <param name="bitmapData">Bitmap data to register</param>
        /// <returns>Id of registered bitmap data</returns>
        public int Register(object bitmapData)
        {
            CheckBitmapData(bitmapData);

            var id = _counter;
            _counter++;
            _register[id] = bitmapData;
            return id;
        }

        /// <summary>
        /// Unregister a existing bitmap
        /// </summary>
        /// <param name="id">Id of registered bitmap data</param>
        public void Unregister(int id)
        {
            _register.Remove(id);
        }

        /// <summary>
        /// Get bitmap data of registered bitmap
        /// </summary>
        /// <param name="id">Id of existing bitmap data</param>
        /// <returns></returns>
        public object Get(int id)
        {
            return _register[id];
        }

        /// <summary>
        /// Set new bitmap data for a already registered bitmap
        /// </summary>
        /// <param name="id">Id of existing bitmap data</param>
        /// <param name="bitmapData">New bitmap data to replace</param>
        /// <returns>True, if replacing worked correct</returns>
        public bool Set(int id, object bitmapData)
        {
            CheckBitmapData(bitmapData);

            if (id < 0 || id >= _counter || !_register.ContainsKey(id))
                return false;

            _register[id] = bitmapData;

            return true;
        }

        /// <summary>
        /// Check bitmap data for correctness
        /// </summary>
        /// <param name="bitmapData">Bitmap data to check</param>
        private void CheckBitmapData(object bitmapData)
        {
            if (bitmapData == null)
                throw new ArgumentException("The bitmap data that is registered is null. Was the image loaded correctly?");

            if (bitmapData is Sprite sprite)
            {
                if (sprite.Atlas < 0 || !_register.ContainsKey(sprite.Atlas))
                {
                    throw new ArgumentException("Sprite has no corresponding atlas bitmap.");
                }
            }
        }
    }
}
