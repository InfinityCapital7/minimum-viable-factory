namespace SoftwareFactory;

/// <summary>
/// Shared handoff object passed from one factory stage to the next.
/// </summary>
public sealed class WorkItem
{
    public required Ticket Ticket { get; init; }

    public required string RepoRoot { get; init; }

    public required string OutputDirectory { get; init; }

    public string Brain { get; init; } = "mock";

    public string CurrentStage { get; set; } = "New";

    public string? Classification { get; set; }

    public string? Plan { get; set; }

    public string? BuildDirectory { get; set; }

    public string ReviewVerdict { get; set; } = "PENDING";

    public string? ReviewNotes { get; set; }

    public List<StageRecord> History { get; } = [];
}

public sealed class StageRecord
{
    public required string Name { get; init; }

    public required bool Ok { get; init; }

    public string? Notes { get; init; }
}
