using System.Text.Json;
using Api.Models;

namespace Api.Services;

public sealed class FileVectorStoreService : IVectorStoreService
{
    private readonly string _artifactPath;
    private List<VectorRecord> _records = [];
    private bool _isLoaded;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public FileVectorStoreService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _artifactPath = ResolveArtifactPath(configuration, environment);
        
        string? directory = Path.GetDirectoryName(_artifactPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task<IReadOnlyList<VectorRecord>> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_isLoaded) return _records;
            
            if (File.Exists(_artifactPath))
            {
                var content = await File.ReadAllTextAsync(_artifactPath, cancellationToken);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    _records = JsonSerializer.Deserialize<List<VectorRecord>>(content) ?? [];
                }
            }
            _isLoaded = true;
            return _records;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SaveAsync(IEnumerable<VectorRecord> records, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            _records = records.ToList();
            var content = JsonSerializer.Serialize(_records, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_artifactPath, content, cancellationToken);
            _isLoaded = true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IReadOnlyList<VectorSearchMatch>> SearchAsync(float[] queryEmbedding, int topK, CancellationToken cancellationToken = default)
    {
        var records = await LoadAsync(cancellationToken);

        var matches = records
            .Select(record => new VectorSearchMatch
            {
                Record = record,
                Score = CalculateCosineSimilarity(queryEmbedding, record.Embedding)
            })
            .OrderByDescending(m => m.Score)
            .Take(topK)
            .ToList();

        return matches;
    }

    private static double CalculateCosineSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length || vector1.Length == 0) return 0;

        double dotProduct = 0;
        double magnitude1 = 0;
        double magnitude2 = 0;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        if (magnitude1 == 0 || magnitude2 == 0) return 0;

        return dotProduct / (Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
    }

    private static string ResolveArtifactPath(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configuredPath = configuration["Challenge:VectorStoreJsonPath"] ?? "Data/vector-store.json";

        return Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(environment.ContentRootPath, configuredPath);
    }
}