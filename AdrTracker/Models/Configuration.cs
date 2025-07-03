using neXn.Ui.Models;
using System.Text.Json.Serialization;

namespace AdrTracker.Models
{
    internal record Configuration
    {
        public Location UserWindowLocation { get; set; }

        [JsonIgnore()]
        internal string DatabaseFile { get; } = "db.nexn";
    }
}
