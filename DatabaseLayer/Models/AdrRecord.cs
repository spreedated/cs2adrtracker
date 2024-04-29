using System;

namespace DatabaseLayer.Models
{
    public record AdrRecord
    {
        public enum Outcomes
        {
            Unknown,
            Loss,
            Win,
            Draw
        }

        public int Id { get; set; }
        public int Value { get; set; }
        public Outcomes Outcome { get; set; }
        public long Timestamp { get; set; }
        public DateTime DateTime => DateTime.UnixEpoch.AddSeconds(this.Timestamp).ToLocalTime();

        public bool IsValid()
        {
            return this.Value != default && this.Timestamp != default;
        }
    }
}