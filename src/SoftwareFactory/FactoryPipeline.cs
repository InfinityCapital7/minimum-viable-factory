using Microsoft.SemanticKernel;

namespace SoftwareFactory;

public sealed class FactoryPipeline
{
    private readonly IReadOnlyList<IFactoryAgent> _agents;

    public FactoryPipeline(IEnumerable<IFactoryAgent> agents)
    {
        _agents = agents.ToList();
    }

    public async Task<WorkItem> RunAsync(WorkItem work, Kernel kernel, CancellationToken cancellationToken = default)
    {
        foreach (var agent in _agents)
        {
            StageLog.Begin(agent.Name);
            work.CurrentStage = agent.Name;
            await agent.ExecuteAsync(work, kernel, cancellationToken);
        }

        return work;
    }
}
