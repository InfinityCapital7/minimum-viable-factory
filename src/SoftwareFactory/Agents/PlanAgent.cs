using Microsoft.SemanticKernel;

namespace SoftwareFactory.Agents;

public sealed class PlanAgent : IFactoryAgent
{
    public string Name => "Plan";

    public async Task ExecuteAsync(WorkItem work, Kernel kernel, CancellationToken cancellationToken = default)
    {
        var criteria = work.Ticket.AcceptanceCriteria.Length == 0
            ? "(none listed)"
            : string.Join("\n- ", work.Ticket.AcceptanceCriteria.Prepend(""));

        var prompt = $"""
            You are the Plan agent in a software factory.
            STAGE: PLAN
            Ticket id: {work.Ticket.Id}
            Title: {work.Ticket.Title}
            Triage: {work.Classification}
            Acceptance criteria:{criteria}

            Write a short numbered plan to ship a HelloWorld console app.
            """;

        var reply = (await kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken)).ToString();
        work.Plan = reply.Trim();
        work.History.Add(new StageRecord { Name = Name, Ok = true, Notes = work.Plan });

        StageLog.Brain(reply);
    }
}
