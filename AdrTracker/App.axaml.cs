using AdrTracker.Logic;
using AdrTracker.ViewModels;
using AdrTracker.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using System;
using System.IO;

namespace AdrTracker
{
    public partial class App : Application
    {
        private readonly static LogEventLevel level = LogEventLevel.Verbose;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
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

            Microsoft.Extensions.Logging.ILogger logger = new SerilogLoggerProvider(Log.Logger).CreateLogger("Application");

            logger.LogTrace("Logger created @ {Time}", DateTime.Now.ToString("G"));

            Globals.Configuration = new(new(Path.Combine(AppContext.BaseDirectory, "config.json")));
            Globals.Configuration.Load();

            logger.LogTrace("Configuration loaded");

            Globals.Database = new(Globals.Configuration.RuntimeConfiguration.DatabaseFile);

            logger.LogTrace("Database connection established");

            if (base.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                BindingPlugins.DataValidators.RemoveAt(0);
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}