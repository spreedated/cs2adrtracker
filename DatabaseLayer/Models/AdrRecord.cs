using System;

namespace DatabaseLayer.Models
{
    public record AdrRecord
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public long UnixTimestamp { get; set; }
        public DateTime DateTime
        {
            get
            {
                return UnixTimeStampToDateTime(this.UnixTimestamp);
            }
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            return DateTime.UnixEpoch.AddSeconds(unixTimeStamp).ToLocalTime();
        }

        public bool IsValid()
        {
            return this.Value != default && this.UnixTimestamp != default;
        }
    }
}
