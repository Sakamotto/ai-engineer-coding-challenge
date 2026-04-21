namespace Api.Models;

public sealed class VectorRecord
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Source { get; init; } = string.Empty;
    public string ChunkText { get; init; } = string.Empty;
    public float[] Embedding { get; init; } = [];
    public Dictionary<string, string> Metadata { get; init; } = [];

    public VectorRecord() { }

    public static VectorRecord Create(string source, string chunkText, float[] embedding)
    {
        if (string.IsNullOrWhiteSpace(chunkText)) throw new ArgumentException("Chunk text cannot be empty.", nameof(chunkText));
        if (embedding == null || embedding.Length == 0) throw new ArgumentException("Embedding cannot be empty.", nameof(embedding));

        return new VectorRecord
        {
            Source = source,
            ChunkText = chunkText,
            Embedding = embedding
        };
    }
}