namespace SoftwareFactory;

internal static class RepoRoot
{
    public static string Find()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var dir = new DirectoryInfo(start);
            while (dir is not null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "tickets", "sample-ticket.json"))
                    || File.Exists(Path.Combine(dir.FullName, "SoftwareFactory.sln")))
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }
        }

        return Directory.GetCurrentDirectory();
    }
}
