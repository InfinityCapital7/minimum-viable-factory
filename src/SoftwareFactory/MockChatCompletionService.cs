using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SoftwareFactory;

/// <summary>
/// Deterministic stand-in for a chat model. No network. No secrets.
/// </summary>
public sealed class MockChatCompletionService : IChatCompletionService
{
    public IReadOnlyDictionary<string, object?> Attributes { get; } =
        new Dictionary<string, object?> { ["provider"] = "mock" };

    public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ChatMessageContent> result =
            [new ChatMessageContent(AuthorRole.Assistant, Compose(chatHistory))];
        return Task.FromResult(result);
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return new StreamingChatMessageContent(AuthorRole.Assistant, Compose(chatHistory));
        await Task.CompletedTask;
    }

    private static string Compose(ChatHistory history)
    {
        var prompt = string.Join('\n', history.Select(message => message.Content ?? string.Empty));
        var ticketId = ExtractTicketId(prompt);
        var stage = DetectStage(prompt);

        return stage switch
        {
            "TRIAGE" =>
                $"Classification: hello-world console app. Priority: P3. Ticket {ticketId} is a greeting that must print the ticket id.",
            "PLAN" =>
                $"1. Scaffold a net8.0 console project under output/HelloWorld.\n2. Print a greeting that includes {ticketId}.\n3. Build and run it; review PASS only if both succeed.",
            "BUILD" =>
                $"Write Program.cs that prints: Hello, World! Built by the software factory for {ticketId}.",
            "REVIEW" =>
                $"PASS if `dotnet build` succeeds and the app output contains {ticketId}.",
            "DONE" =>
                $"Ticket {ticketId} is complete. Persist factory-status.json.",
            _ =>
                $"Acknowledged ticket {ticketId}."
        };
    }

    private static string DetectStage(string prompt)
    {
        foreach (var stage in new[] { "TRIAGE", "PLAN", "BUILD", "REVIEW", "DONE" })
        {
            if (prompt.Contains($"STAGE: {stage}", StringComparison.OrdinalIgnoreCase)
                || prompt.Contains($"the {stage} agent", StringComparison.OrdinalIgnoreCase))
            {
                return stage;
            }
        }

        return "UNKNOWN";
    }

    private static string ExtractTicketId(string prompt)
    {
        var match = Regex.Match(prompt, @"SF-\d+", RegexOptions.IgnoreCase);
        return match.Success ? match.Value.ToUpperInvariant() : "SF-101";
    }
}
