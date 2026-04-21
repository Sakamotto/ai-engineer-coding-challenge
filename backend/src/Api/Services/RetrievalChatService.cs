using System.Text.Json;
using Api.Contracts;
using Api.Models;
using OpenAI.Chat;

namespace Api.Services;

public sealed class RetrievalChatService : IRetrievalChatService
{
    private readonly ChatClient _chatClient;
    private readonly IEmbeddingService _embedding;
    private readonly IVectorStoreService _vectorStore;

    public RetrievalChatService(IConfiguration config, IEmbeddingService embedding, IVectorStoreService vectorStore)
    {
        string apiKey = config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("API Key missing");
        _chatClient = new ChatClient("gpt-4o", apiKey);
        _embedding = embedding;
        _vectorStore = vectorStore;
    }

    public async Task<ChatResponse> GenerateResponseAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        var chatMessages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a helpful grocery store assistant. Help employees with the Store SOP. When answering specific procedures or rules, ALWAYS search the SOP using your tools and cite the source section in your final answer.")
        };

        foreach (var msg in request.Messages)
        {
            if (msg.Role == "user") chatMessages.Add(new UserChatMessage(msg.Content));
            else if (msg.Role == "assistant") chatMessages.Add(new AssistantChatMessage(msg.Content));
            else if (msg.Role == "system") chatMessages.Add(new SystemChatMessage(msg.Content));
        }

        ChatTool searchTool = ChatTool.CreateFunctionTool(
            functionName: "SearchSOP",
            functionDescription: "Searches the SOP document by semantic query and returns the matching text chunks and their sources.",
            functionParameters: BinaryData.FromString(@"{
                ""type"": ""object"",
                ""properties"": {
                    ""query"": {
                        ""type"": ""string"",
                        ""description"": ""The specific query to search the SOP, e.g. 'store hours' or 'loss prevention'""
                    }
                },
                ""required"": [""query""]
            }")
        );

        ChatCompletionOptions options = new();
        if (request.UseTools)
        {
            options.Tools.Add(searchTool);
        }

        var completion = await _chatClient.CompleteChatAsync(chatMessages, options, cancellationToken);
        var citations = new List<CitationDto>();
        var toolCalls = new List<string>();

        if (completion.Value.FinishReason == ChatFinishReason.ToolCalls)
        {
            chatMessages.Add(new AssistantChatMessage(completion.Value));

            foreach (var call in completion.Value.ToolCalls)
            {
                if (call.FunctionName == "SearchSOP")
                {
                    toolCalls.Add(call.FunctionName);
                    
                    using var doc = JsonDocument.Parse(call.FunctionArguments);
                    string query = doc.RootElement.GetProperty("query").GetString() ?? "";
                    
                    var queryEmbedding = await _embedding.EmbedAsync(query, cancellationToken);
                    var matches = await _vectorStore.SearchAsync(queryEmbedding, topK: 3, cancellationToken);
                    
                    var searchResults = new System.Text.StringBuilder();
                    searchResults.AppendLine("Search Results from SOP:");
                    foreach (var match in matches)
                    {
                        searchResults.AppendLine($"Source Section: {match.Record.Source}");
                        searchResults.AppendLine($"Snippet: {match.Record.ChunkText}");
                        searchResults.AppendLine("---");
                        
                        citations.Add(new CitationDto
                        {
                            Source = match.Record.Source,
                            Snippet = match.Record.ChunkText.Length > 120 
                                ? match.Record.ChunkText[..120] + "..." 
                                : match.Record.ChunkText
                        });
                    }

                    chatMessages.Add(new ToolChatMessage(call.Id, searchResults.ToString()));
                }
            }
            
            // Second call with tool results
            completion = await _chatClient.CompleteChatAsync(chatMessages, cancellationToken: cancellationToken);
        }

        return new ChatResponse
        {
            ConversationId = request.ConversationId,
            AssistantMessage = completion.Value.Content[0].Text,
            Status = "Success",
            IsPlaceholder = false,
            ToolCalls = toolCalls,
            Citations = citations,
        };
    }
}
