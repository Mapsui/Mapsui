using System;
using Mapsui.Logging;

namespace Mapsui.Samples.Eto
{
    class LogModel
    {
        public Exception? Exception { get; set; }
        public string? Message { get; set; }
        public LogLevel LogLevel { get; set; }
    }
}
