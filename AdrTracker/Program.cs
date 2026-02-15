using AdrTracker.Logic;
using Avalonia;
using Microsoft.Extensions.Logging;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.MaterialDesign;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using System;
using System.IO;

namespace AdrTracker
{
    internal static class Program
    {
        private static LogEventLevel level = LogEventLevel.Verbose;
        private static Microsoft.Extensions.Logging.ILogger logger;

        private static void LoadLogger()
        {
#if !DEBUG
            level = LogEventLevel.Information;
#endif

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.Console(restrictedToMinimumLevel: level)
                .WriteTo.Debug(restrictedToMinimumLevel: level)
                .Enrich.WithProperty("application", typeof(App).Assembly.GetName().Name)
                .Enrich.WithProperty("version", typeof(App).Assembly.GetName().Version)
#if DEBUG
                .Enrich.WithProperty("debug", true)
#endif
                .CreateLogger();

            logger = new SerilogLoggerProvider(Log.Logger).CreateLogger("Application");

            logger.LogTrace("Logger created");
        }

        private static void LoadConfig()
        {
            if (OperatingSystem.IsWindows())
            {
                Globals.AppLocalBaseUserPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "neXn-Systems", "CS2-ADR-Tracker");
            }
            else
            {
                Globals.AppLocalBaseUserPath = AppContext.BaseDirectory;
            }

            Globals.Configuration = new(new(Path.Combine(Globals.AppLocalBaseUserPath, "config.json"))
            {
                Autoload = false
            });
            Globals.Configuration.Load().Wait();

            logger.LogTrace("Configuration loaded");
        }

        private static void LoadDatabase()
        {
            Globals.Database = new(Path.Combine(Globals.AppLocalBaseUserPath, Globals.Configuration.RuntimeConfiguration.DatabaseFile));
            logger.LogTrace("Database connection established");
        }

        [STAThread]
        public static void Main(string[] args)
        {
            LoadLogger();
            LoadConfig();
            LoadDatabase();

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            IconProvider.Current.Register<MaterialDesignIconProvider>();

            return AppBuilder.Configure<App>()
                        .UsePlatformDetect()
                        .WithInterFont()
                        .LogToTrace();
        }
    }
}
