using HTMLBuilder.Ui;

namespace HTMLBuilder;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var settings = AppSettings.Load();
        Localizer.CurrentLanguage = settings.Language;
        Application.Run(new MainForm(settings));
    }
}
