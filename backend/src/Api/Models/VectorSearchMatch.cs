namespace Api.Models;

public sealed class VectorSearchMatch
{
    public required VectorRecord Record { get; init; }

    public double Score { get; init; }
}