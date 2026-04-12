using Avalonia;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StoreSyncFront;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            LogException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");
        };

        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            LogException(e.Exception, "TaskScheduler.UnobservedTaskException");
            e.SetObserved();
        };

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            LogException(e, "Main Thread");
            throw;
        }
    } 
    
    public static void LogException(Exception ex, string source)
    {
        try
        {
            var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");
            var message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR in {source}:\n{ex}\n----------------------------------------\n";
            File.AppendAllText(logFilePath, message);
        }
        catch
        {
            // Fallback se não conseguir escrever o log
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}