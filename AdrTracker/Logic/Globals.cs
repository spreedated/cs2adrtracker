using AdrTracker.Models;
using DatabaseLayer.DataLayer;
using neXn.Lib.ConfigurationHandler;

namespace AdrTracker.Logic
{
    internal static class Globals
    {
        internal static ConfigurationHandler<Configuration> Configuration { get; set; }
        internal static Database Database { get; set; }
    }
}
