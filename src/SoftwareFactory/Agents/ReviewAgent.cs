using System.Diagnostics;
using Microsoft.SemanticKernel;

namespace SoftwareFactory.Agents;

public sealed class ReviewAgent : IFactoryAgent
{
    public string Name => "Review";

    public async Task ExecuteAsync(WorkItem work, Kernel kernel, CancellationToken cancellationToken = default)
    {
        var prompt = $"""
            You are the Review agent in a software factory.
            STAGE: REVIEW
            Ticket id: {work.Ticket.Id}
            Build directory: {work.BuildDirectory}

            What must be true for a PASS?
            """;

        var reply = (await kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken)).ToString();
        StageLog.Brain(reply);

        if (string.IsNullOrWhiteSpace(work.BuildDirectory) || !Directory.Exists(work.BuildDirectory))
        {
            RecordFail(work, "No HelloWorld project to review.");
            return;
        }

        var build = await RunDotnetAsync(work.BuildDirectory, ["build", "--nologo"], cancellationToken);
        StageLog.Info($"dotnet build -> {(build.Ok ? "SUCCESS" : "FAIL")} (exit {build.ExitCode})");
        if (!build.Ok)
        {
            RecordFail(work, $"dotnet build failed:\n{build.Output}");
            return;
        }

        var run = await RunDotnetAsync(work.BuildDirectory, ["run", "--no-build", "--nologo"], cancellationToken);
        var greeting = run.Output.Trim();
        StageLog.Info($"dotnet run -> {greeting}");

        var mentionsTicket = greeting.Contains(work.Ticket.Id, StringComparison.Ordinal);
        if (!run.Ok || !mentionsTicket)
        {
            RecordFail(work, $"App did not greet with ticket id {work.Ticket.Id}. Output: {greeting}");
            return;
        }

        work.ReviewVerdict = "PASS";
        work.ReviewNotes = $"Build succeeded. Greeting includes {work.Ticket.Id}.";
        work.History.Add(new StageRecord { Name = Name, Ok = true, Notes = work.ReviewNotes });
        StageLog.Info("Verdict: PASS");
    }

    private void RecordFail(WorkItem work, string notes)
    {
        work.ReviewVerdict = "FAIL";
        work.ReviewNotes = notes;
        work.History.Add(new StageRecord { Name = Name, Ok = false, Notes = notes });
        StageLog.Info("Verdict: FAIL");
        StageLog.Info(notes);
    }

    private static async Task<(bool Ok, int ExitCode, string Output)> RunDotnetAsync(
        string workingDirectory,
        string[] args,
        CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var arg in args)
        {
            start.ArgumentList.Add(arg);
        }

        using var process = Process.Start(start)
            ?? throw new InvalidOperationException("Failed to start dotnet.");

        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        var output = string.Join('\n', new[] { stdout, stderr }.Where(s => !string.IsNullOrWhiteSpace(s)));
        return (process.ExitCode == 0, process.ExitCode, output);
    }
}
