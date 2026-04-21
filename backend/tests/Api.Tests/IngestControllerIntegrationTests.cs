using Api.Contracts;
using Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace Api.Tests;

public class IngestControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IngestControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_Ingest_ShouldReturnSuccess_WhenPipelineRunsProperly()
    {
        // Arrange
        // Here we mock OpenAI simply to avoid paying unnecessary API usage fees during CI/CD.
        var mockEmbeddingService = new Mock<IEmbeddingService>();
        mockEmbeddingService
            .Setup(x => x.EmbedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new float[] { 0.1f, 0.2f, 0.3f }); // Injected fake vector

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmbeddingService));
                if (descriptor != null) services.Remove(descriptor);
                
                services.AddSingleton(mockEmbeddingService.Object);
            });
        }).CreateClient();

        // Locating the repository root from the bin/Debug/net10.0 output folder
        var currentDir = Directory.GetCurrentDirectory();
        var rootDir = Directory.GetParent(currentDir)?.Parent?.Parent?.Parent?.Parent?.Parent?.FullName ?? "";
        var requestPath = Path.Combine(rootDir, "knowledge-base", "Grocery_Store_SOP.md");

        var request = new IngestRequest { SourcePath = requestPath };

        // Act
        var response = await client.PostAsJsonAsync("/api/ingest", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<IngestResponse>();
        result.Should().NotBeNull();
        result!.Accepted.Should().BeTrue();
        result.IsPlaceholder.Should().BeFalse();
        
        // Business Assert: The RAG pipeline successfully executed ingestions and Vector persistence
        result.ChunksCreated.Should().BeGreaterThan(0);
        result.RecordsPersisted.Should().BeGreaterThan(0);
    }
}
