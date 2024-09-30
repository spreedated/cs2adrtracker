using Cs2GlobalAdrTracker.Logic;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Windows;

namespace Cs2GlobalAdrTracker
{
    public partial class App : Application
    {
        private readonly static LogEventLevel level = LogEventLevel.Verbose;
        private readonly static string configPath = Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory), "config.json");
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                .WriteTo.Console(restrictedToMinimumLevel: level)
                .WriteTo.Debug(restrictedToMinimumLevel: level)
                .Enrich.WithProperty("application", typeof(App).Assembly.GetName().Name)
                .Enrich.WithProperty("version", typeof(App).Assembly.GetName().Version)
                .CreateLogger();

            Log.Debug($"Logger created @ {DateTime.Now:G}");

            Globals.Configuration = new(new(configPath));
            Globals.Configuration.Load();

            Log.Verbose("Configuration loaded");

            Globals.Database = new(Globals.Configuration.RuntimeConfiguration.DatabaseFile);

            Log.Verbose("Database connection established");
        }
    }
}