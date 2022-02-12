using System;

namespace Mapsui.Rendering
{
    /// <summary>
    /// Exception thrown when a layer rendering fails
    /// </summary>
    public class RenderException : Exception
    {
        /// <summary>
        /// Exception thrown when layer rendering has failed
        /// </summary>
        /// <param name="message"></param>
        public RenderException(string message) : base(message)
        {
        }

        /// <summary>
        /// Exception thrown when layer rendering has failed
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public RenderException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}