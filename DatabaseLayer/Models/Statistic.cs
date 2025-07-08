namespace DatabaseLayer.Models
{
    public sealed record Statistic
    {
        public int Ties { get; init; }
        public int Defeats { get; init; }
        public int Victories { get; init; }
    }
}
