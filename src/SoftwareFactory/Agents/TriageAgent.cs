using Microsoft.SemanticKernel;

namespace SoftwareFactory.Agents;

public sealed class TriageAgent : IFactoryAgent
{
    public string Name => "Triage";

    public async Task ExecuteAsync(WorkItem work, Kernel kernel, CancellationToken cancellationToken = default)
    {
        var prompt = $"""
            You are the Triage agent in a software factory.
            STAGE: TRIAGE
            Ticket id: {work.Ticket.Id}
            Title: {work.Ticket.Title}
            Description: {work.Ticket.Description}

            Classify the ticket in one or two sentences and assign a priority.
            """;

        var reply = (await kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken)).ToString();
        work.Classification = reply.Trim();
        work.History.Add(new StageRecord { Name = Name, Ok = true, Notes = work.Classification });

        StageLog.Brain(reply);
        StageLog.Info($"Ticket {work.Ticket.Id}: {work.Ticket.Title}");
    }
}
