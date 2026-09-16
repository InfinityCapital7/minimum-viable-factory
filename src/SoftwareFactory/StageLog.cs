namespace SoftwareFactory;

internal static class StageLog
{
    public static void Begin(string stage)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 64));
        Console.WriteLine($"  {stage}");
        Console.WriteLine(new string('=', 64));
    }

    public static void Info(string message) => Console.WriteLine($"  {message}");

    public static void Brain(string reply)
    {
        var trimmed = reply.Trim();
        if (trimmed.Length == 0)
        {
            return;
        }

        foreach (var line in trimmed.Split('\n'))
        {
            Console.WriteLine($"  brain> {line.TrimEnd()}");
        }
    }
}
