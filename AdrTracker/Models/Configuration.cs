using neXn.Ui.Models;
using System.Text.Json.Serialization;

namespace AdrTracker.Models
{
    internal record Configuration
    {
        public Location UserWindowLocation { get; set; }

        public bool ShowQuickStatsChart { get; set; } = true;
        public bool ShowQuickStatsString { get; set; } = true;

        [JsonIgnore()]
        internal string DatabaseFile { get; } = "db.nexn";
    }
}
