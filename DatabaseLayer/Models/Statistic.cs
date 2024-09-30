namespace DatabaseLayer.Models
{
    public sealed record Statistic
    {
        public int Draws { get; init; }
        public int Losses { get; init; }
        public int Wins { get; init; }
    }
}
