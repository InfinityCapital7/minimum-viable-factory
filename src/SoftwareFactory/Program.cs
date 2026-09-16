using System.Text.Json;
using SoftwareFactory;
using SoftwareFactory.Agents;

var repoRoot = RepoRoot.Find();
var ticketPath = args.Length > 0
    ? args[0]
    : Path.Combine(repoRoot, "tickets", "sample-ticket.json");

if (!Path.IsPathRooted(ticketPath))
{
    ticketPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ticketPath));
    if (!File.Exists(ticketPath))
    {
        ticketPath = Path.GetFullPath(Path.Combine(repoRoot, args[0]));
    }
}

if (!File.Exists(ticketPath))
{
    Console.Error.WriteLine($"Ticket file not found: {ticketPath}");
    return 1;
}

var ticket = JsonSerializer.Deserialize<Ticket>(
    await File.ReadAllTextAsync(ticketPath),
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

if (ticket is null || string.IsNullOrWhiteSpace(ticket.Id))
{
    Console.Error.WriteLine("Ticket JSON is missing a required id.");
    return 1;
}

var (kernel, brain) = KernelFactory.Create();
var outputDir = Path.Combine(repoRoot, "output");
Directory.CreateDirectory(outputDir);

Console.WriteLine("Minimum Viable Factory");
Console.WriteLine($"Brain: {(brain == "mock" ? "deterministic mock (no API key)" : "OpenAI-compatible")}");
Console.WriteLine($"Ticket: {Path.GetRelativePath(repoRoot, ticketPath).Replace('\\', '/')} ({ticket.Id})");

var work = new WorkItem
{
    Ticket = ticket,
    RepoRoot = repoRoot,
    OutputDirectory = outputDir,
    Brain = brain
};

var pipeline = new FactoryPipeline(
[
    new TriageAgent(),
    new PlanAgent(),
    new BuildAgent(),
    new ReviewAgent(),
    new DoneAgent()
]);

await pipeline.RunAsync(work, kernel);

Console.WriteLine();
Console.WriteLine(work.ReviewVerdict == "PASS"
    ? "Factory finished: Review PASS."
    : "Factory finished: Review FAIL.");

return work.ReviewVerdict == "PASS" ? 0 : 2;
