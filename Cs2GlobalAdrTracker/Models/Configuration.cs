using System.Text.Json.Serialization;

namespace Cs2GlobalAdrTracker.Models
{
    internal record Configuration
    {
        public WindowLocation WindowStartupLocation { get; set; }

        [JsonIgnore()]
        internal string Databasefile { get; } = "db.nexn";
    }
}
