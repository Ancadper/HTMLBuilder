using HTMLBuilder.Ui;

namespace HTMLBuilder;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        if (args.Contains("--verify-rules", StringComparer.OrdinalIgnoreCase))
        {
            return RuleComplianceVerifier.Run(Console.Out);
        }

        var settings = AppSettings.Load();
        Localizer.CurrentLanguage = settings.Language;
        Application.Run(new MainForm(settings));
        return 0;
    }
}
