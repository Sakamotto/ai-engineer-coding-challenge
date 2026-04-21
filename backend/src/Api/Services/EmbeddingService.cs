using Api.Models;
using OpenAI.Embeddings;

namespace Api.Services;

public sealed class EmbeddingService : IEmbeddingService
{
    private readonly EmbeddingClient _client;

    public EmbeddingService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("Missing OpenAI API Key in configuration.");
        // Using text-embedding-3-small as it is modern, fast, and the current default recommended by OpenAI
        _client = new EmbeddingClient("text-embedding-3-small", apiKey);
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        var response = await _client.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
        return response.Value.ToFloats().ToArray();
    }
}
