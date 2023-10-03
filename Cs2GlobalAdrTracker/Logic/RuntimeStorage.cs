using Cs2GlobalAdrTracker.Models;
using DatabaseLayer.DataLayer;
using neXn.Lib.ConfigurationHandler;

namespace Cs2GlobalAdrTracker.Logic
{
    internal static class RuntimeStorage
    {
        internal static ConfigurationHandler<Configuration> Configuration { get; set; }
        internal static Database Database { get; set; }
    }
}
