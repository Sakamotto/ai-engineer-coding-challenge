using Api.Contracts;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class IngestController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IChunkingService _chunking;
    private readonly IEmbeddingService _embedding;
    private readonly IVectorStoreService _vectorStore;

    public IngestController(
        IConfiguration configuration,
        IChunkingService chunking,
        IEmbeddingService embedding,
        IVectorStoreService vectorStore)
    {
        _configuration = configuration;
        _chunking = chunking;
        _embedding = embedding;
        _vectorStore = vectorStore;
    }

    [HttpPost]
    public async Task<ActionResult<IngestResponse>> Post([FromBody] IngestRequest? request, CancellationToken cancellationToken)
    {
        var configuredSourcePath = _configuration["Challenge:SourceDocumentPath"] ?? "../../../knowledge-base/Grocery_Store_SOP.md";
        var sourcePath = string.IsNullOrWhiteSpace(request?.SourcePath) ? configuredSourcePath : request.SourcePath;
        var vectorStorePath = _configuration["Challenge:VectorStoreJsonPath"] ?? "Data/vector-store.json";

        // Attempts to find the file at the exact path, otherwise attempts to fix the Path by locating the Repository root (Anti-Crash protection)
        if (!System.IO.File.Exists(sourcePath)) 
        {
            var fallbackName = "Grocery_Store_SOP.md";
            var rootDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while(rootDir != null && !Directory.Exists(Path.Combine(rootDir.FullName, "knowledge-base")))
            {
                rootDir = rootDir.Parent;
            }
            
            if (rootDir != null) 
            {
                sourcePath = Path.Combine(rootDir.FullName, "knowledge-base", fallbackName);
            }
        }

        if (!System.IO.File.Exists(sourcePath))
            return BadRequest(new { error = $"Source file not found at {sourcePath}" });

        string documentText = await System.IO.File.ReadAllTextAsync(sourcePath, cancellationToken);
        
        var chunks = await _chunking.ChunkAsync(documentText, "Grocery Store Operations Manual", cancellationToken);
        
        var records = new List<VectorRecord>();
        foreach (var chunk in chunks)
        {
            var vector = await _embedding.EmbedAsync(chunk.Content, cancellationToken);
            records.Add(VectorRecord.Create(chunk.Source, chunk.Content, vector));
        }

        await _vectorStore.SaveAsync(records, cancellationToken);

        return Ok(new IngestResponse
        {
            Accepted = true,
            Message = "Document loaded, chunked and converted to local embeddings successfully.",
            SourcePath = sourcePath,
            ChunksCreated = chunks.Count,
            RecordsPersisted = records.Count,
            VectorStorePath = vectorStorePath,
            IsPlaceholder = false
        });
    }
}