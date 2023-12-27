using System;

namespace DatabaseLayer.Models
{
    public record AdrRecord
    {
        public enum Outcomes
        {
            Unknown,
            Lose,
            Win,
            Draw
        }

        public int Id { get; set; }
        public int Value { get; set; }
        public Outcomes Outcome { get; set; }
        public long UnixTimestamp { get; set; }
        public DateTime DateTime => DateTime.UnixEpoch.AddSeconds(this.UnixTimestamp).ToLocalTime();

        public bool IsValid()
        {
            return this.Value != default && this.UnixTimestamp != default;
        }
    }
}