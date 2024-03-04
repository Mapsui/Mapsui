using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Mapsui.Logging;

namespace Mapsui.Extensions;

public static class Catch
{
    public static void Exceptions(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            Logger.Log(LogLevel.Error, e.Message, e);
        }
    }

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

    [SuppressMessage("Usage", "VSTHRD110:Observe result of async calls")]
    public static void TaskRun(Action func)
    {
        Task.Run(() =>
        {
            try
            {
                func();
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e.Message, e);
            }
        });
    }

    [SuppressMessage("Usage", "VSTHRD110:Observe result of async calls")]
    public static void TaskRun(Func<Task> func)
    {
        Task.Run(async () =>
        {
            try
            {
                await func();
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e.Message, e);
            }
        });
    }
}
