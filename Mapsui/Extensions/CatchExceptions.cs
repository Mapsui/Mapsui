using System;
using System.Threading.Tasks;
using Mapsui.Logging;

namespace Mapsui.Extensions
{
    public static class Catch
    {
        public static void Exceptions(Func<Task> func)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await func();
                }
                catch (Exception e)
                {
                    Logging.Logger.Log(LogLevel.Error, e.Message, e);
                }
            });
        }
    }
}
