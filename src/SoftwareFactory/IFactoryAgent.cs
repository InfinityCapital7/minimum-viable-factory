using Microsoft.SemanticKernel;

namespace SoftwareFactory;

public interface IFactoryAgent
{
    string Name { get; }

    Task ExecuteAsync(WorkItem work, Kernel kernel, CancellationToken cancellationToken = default);
}
