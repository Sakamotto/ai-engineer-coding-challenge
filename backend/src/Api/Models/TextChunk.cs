namespace Api.Models;

public sealed class TextChunk
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Source { get; init; } = string.Empty;
    public int Index { get; init; }
    public string Content { get; init; } = string.Empty;

    public TextChunk() { }

    public static TextChunk Create(string source, int index, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty.");

        return new TextChunk
        {
            Source = string.IsNullOrWhiteSpace(source) ? "Unknown Source" : source,
            Index = index,
            Content = content.Trim()
        };
    }
}