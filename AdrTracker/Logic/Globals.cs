using AdrTracker.Models;
using DatabaseLayer.DataLayer;
using neXn.Lib.ConfigurationHandler;
using System.Reflection;

namespace AdrTracker.Logic
{
    internal static class Globals
    {
        internal static Assembly Assembly { get; } = typeof(Globals).Assembly;
        internal static ConfigurationHandler<Configuration> Configuration { get; set; }
        internal static Database Database { get; set; }
    }
}
