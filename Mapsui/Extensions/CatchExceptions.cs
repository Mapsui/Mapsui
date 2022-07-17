using System;
using System.Threading.Tasks;
using Mapsui.Logging;

namespace Mapsui.Extensions
{
    public static class Catch
    {
        public static async void Exceptions(Func<Task> func)
        {
            try
            {
                await func();
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e.Message, e);
            }
        }
    }
}
