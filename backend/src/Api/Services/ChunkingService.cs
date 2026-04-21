using Api.Models;
using System.Text;

namespace Api.Services;

public sealed class ChunkingService : IChunkingService
{
    public Task<IReadOnlyList<TextChunk>> ChunkAsync(string sourceText, string sourceName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourceText))
            return Task.FromResult<IReadOnlyList<TextChunk>>([]);

        var chunks = new List<TextChunk>();
        var lines = sourceText.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
        
        string currentHeader = sourceName;
        var currentChunkContent = new StringBuilder();
        int chunkIndex = 0;

        foreach (var line in lines)
        {
            if (line.StartsWith("## ") || line.StartsWith("# "))
            {
                if (currentChunkContent.Length > 0 && !string.IsNullOrWhiteSpace(currentChunkContent.ToString()))
                {
                    chunks.Add(TextChunk.Create(currentHeader, chunkIndex++, currentChunkContent.ToString()));
                    currentChunkContent.Clear();
                }
                currentHeader = line.TrimStart('#', ' ').Trim();
            }
            
            currentChunkContent.AppendLine(line);
        }

        if (currentChunkContent.Length > 0 && !string.IsNullOrWhiteSpace(currentChunkContent.ToString()))
        {
            chunks.Add(TextChunk.Create(currentHeader, chunkIndex++, currentChunkContent.ToString()));
        }

        return Task.FromResult<IReadOnlyList<TextChunk>>(chunks);
    }
}
