using System.Text.Json;
using Microsoft.SemanticKernel;

namespace SoftwareFactory.Agents;

public sealed class DoneAgent : IFactoryAgent
{
    public string Name => "Done";

    public async Task ExecuteAsync(WorkItem work, Kernel kernel, CancellationToken cancellationToken = default)
    {
        var prompt = $"""
            You are the Done agent in a software factory.
            STAGE: DONE
            Ticket id: {work.Ticket.Id}
            Review: {work.ReviewVerdict}

            Confirm how the run should be recorded.
            """;

        var reply = (await kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken)).ToString();
        StageLog.Brain(reply);

        Directory.CreateDirectory(work.OutputDirectory);
        var statusPath = Path.Combine(work.OutputDirectory, "factory-status.json");
        var relative = Path.GetRelativePath(work.RepoRoot, statusPath).Replace('\\', '/');
        var passed = work.ReviewVerdict == "PASS";

        work.History.Add(new StageRecord
        {
            Name = Name,
            Ok = passed,
            Notes = relative
        });

        var status = new
        {
            ticketId = work.Ticket.Id,
            title = work.Ticket.Title,
            status = passed ? "Done" : "Failed",
            review = work.ReviewVerdict,
            reviewNotes = work.ReviewNotes,
            brain = work.Brain,
            helloWorldPath = work.BuildDirectory is null
                ? null
                : Path.GetRelativePath(work.RepoRoot, work.BuildDirectory).Replace('\\', '/'),
            completedAt = DateTimeOffset.UtcNow,
            stages = work.History.Select(stage => new
            {
                name = stage.Name,
                ok = stage.Ok,
                notes = stage.Notes
            })
        };

        var json = JsonSerializer.Serialize(status, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(statusPath, json + Environment.NewLine, cancellationToken);

        StageLog.Info($"Wrote {relative}");
        StageLog.Info($"Status: {status.status}  Review: {work.ReviewVerdict}");
    }
}
