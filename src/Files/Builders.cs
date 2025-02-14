public static partial class Files
{
    public static class Builders
    {
        public static List<string> steamids = new List<string>();
        private static readonly string filepath = Path.Combine(Plugin.Instance.ModuleDirectory, "builders.txt");
        private static readonly char[] separator = ['#', '/', ' '];

        public static void Load()
        {
            if (!File.Exists(filepath) || String.IsNullOrEmpty(File.ReadAllText(filepath)))
            {
                File.WriteAllText(filepath, "# List of builders\n76561197960287930 // example");
                steamids = new List<string>();
                return;
            }

            var builders = new List<string>();

            foreach (var line in File.ReadAllLines(filepath))
            {
                string? steamId = line.Split(separator, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

                if (!string.IsNullOrEmpty(steamId))
                    builders.Add(steamId);
            }

            steamids = builders;
        }

        public static void Save(List<string> builders)
        {
            var lines = File.ReadAllLines(filepath).ToList();
            var updatedLines = new List<string>();
            var steamIdToLineMap = new Dictionary<string, string>();

            foreach (var line in lines)
            {
                string? steamId = line.Split(separator, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

                if (!string.IsNullOrEmpty(steamId))
                    steamIdToLineMap[steamId] = line;

                else updatedLines.Add(line);
            }

            foreach (var steamId in builders)
            {
                if (steamIdToLineMap.ContainsKey(steamId))
                    updatedLines.Add(steamIdToLineMap[steamId]);

                else updatedLines.Add($"{steamId} // added by system");
            }

            File.WriteAllLines(filepath, updatedLines);
        }
    }
}