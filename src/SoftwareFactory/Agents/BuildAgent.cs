using Microsoft.SemanticKernel;

namespace SoftwareFactory.Agents;

public sealed class BuildAgent : IFactoryAgent
{
    public string Name => "Build";

    public async Task ExecuteAsync(WorkItem work, Kernel kernel, CancellationToken cancellationToken = default)
    {
        var prompt = $"""
            You are the Build agent in a software factory.
            STAGE: BUILD
            Ticket id: {work.Ticket.Id}
            Plan: {work.Plan}

            Describe the Program.cs greeting that must include the ticket id.
            """;

        var reply = (await kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken)).ToString();
        StageLog.Brain(reply);

        var projectDir = Path.Combine(work.OutputDirectory, "HelloWorld");
        Directory.CreateDirectory(projectDir);

        var csprojPath = Path.Combine(projectDir, "HelloWorld.csproj");
        var programPath = Path.Combine(projectDir, "Program.cs");

        await File.WriteAllTextAsync(csprojPath, HelloWorldProject, cancellationToken);
        await File.WriteAllTextAsync(
            programPath,
            $$"""
            Console.WriteLine("Hello, World! Built by the software factory for {{work.Ticket.Id}}.");

            """,
            cancellationToken);

        work.BuildDirectory = projectDir;
        work.History.Add(new StageRecord
        {
            Name = Name,
            Ok = true,
            Notes = $"Wrote {Rel(work.RepoRoot, csprojPath)} and {Rel(work.RepoRoot, programPath)}"
        });

        StageLog.Info($"Wrote {Rel(work.RepoRoot, csprojPath)}");
        StageLog.Info($"Wrote {Rel(work.RepoRoot, programPath)}");
    }

    private static string Rel(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    private const string HelloWorldProject =
        """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <OutputType>Exe</OutputType>
            <TargetFramework>net8.0</TargetFramework>
            <ImplicitUsings>enable</ImplicitUsings>
            <Nullable>enable</Nullable>
            <RootNamespace>HelloWorld</RootNamespace>
          </PropertyGroup>
        </Project>
        """;
}
