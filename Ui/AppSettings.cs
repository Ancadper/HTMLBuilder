namespace HTMLBuilder.Ui;

public sealed class AppSettings
{
    private const string AppFolderName = "HTMLBuilder";
    private const string SettingsFileName = "settings.ini";

    public string Language { get; set; } = Localizer.English;
    public bool HasAskedLanguage { get; set; }

    public static string SettingsPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AppFolderName,
            SettingsFileName);

    public static AppSettings Load()
    {
        var settings = new AppSettings();
        if (!File.Exists(SettingsPath))
        {
            return settings;
        }

        foreach (var rawLine in File.ReadAllLines(SettingsPath))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith(';') || line.StartsWith('['))
            {
                continue;
            }

            var separator = line.IndexOf('=');
            if (separator < 0)
            {
                continue;
            }

            var key = line[..separator].Trim();
            var value = line[(separator + 1)..].Trim();

            if (key.Equals("language", StringComparison.OrdinalIgnoreCase))
            {
                settings.Language = Localizer.NormalizeLanguage(value);
            }
            else if (key.Equals("hasAskedLanguage", StringComparison.OrdinalIgnoreCase)
                && bool.TryParse(value, out var hasAskedLanguage))
            {
                settings.HasAskedLanguage = hasAskedLanguage;
            }
        }

        return settings;
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllLines(
            SettingsPath,
            [
                "[Preferences]",
                $"language={Localizer.NormalizeLanguage(Language)}",
                $"hasAskedLanguage={HasAskedLanguage}"
            ]);
    }
}
