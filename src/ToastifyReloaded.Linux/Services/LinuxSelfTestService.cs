using ToastifyReloaded.Linux.Models;

namespace ToastifyReloaded.Linux.Services;

public static class LinuxSelfTestService
{
    public static async Task<int> RunAsync()
    {
        var failures = new List<string>();

        void Check(
            bool condition,
            string name)
        {
            if (condition)
            {
                Console.WriteLine($"PASS: {name}");
            }
            else
            {
                Console.Error.WriteLine($"FAIL: {name}");
                failures.Add(name);
            }
        }

        try
        {
            Console.WriteLine(
                "Toastify Reloaded Linux 1.4.0 self-test");

            Console.WriteLine(
                $"Runtime: {Environment.Version}");

            Console.WriteLine(
                $"OS: {Environment.OSVersion}");

            Check(
                LinuxUpdateService.CurrentTag ==
                "v1.4.0-linux",
                "stable version constant");

            var preview4 =
                LinuxUpdateService.ParseTag(
                    "v1.4.0-linux-preview.4");

            var rc1 =
                LinuxUpdateService.ParseTag(
                    "v1.4.0-linux-rc.1");

            var stable =
                LinuxUpdateService.ParseTag(
                    "v1.4.0-linux");

            var nextPreview =
                LinuxUpdateService.ParseTag(
                    "v1.4.1-linux-preview.1");

            var nextRc =
                LinuxUpdateService.ParseTag(
                    "v1.4.1-linux-rc.1");

            var nextStable =
                LinuxUpdateService.ParseTag(
                    "v1.4.1-linux");

            Check(
                preview4 is not null &&
                rc1 is not null &&
                preview4.CompareTo(rc1) < 0,
                "preview sorts before RC");

            Check(
                rc1 is not null &&
                stable is not null &&
                rc1.CompareTo(stable) < 0,
                "RC sorts before stable");

            Check(
                stable is not null &&
                nextStable is not null &&
                stable.CompareTo(nextStable) < 0,
                "semantic stable versions sort correctly");

            Check(
                stable is not null &&
                nextPreview is not null &&
                !LinuxUpdateService.IsEligibleUpdateCandidate(
                    stable,
                    nextPreview),
                "stable channel rejects future preview");

            Check(
                stable is not null &&
                nextRc is not null &&
                !LinuxUpdateService.IsEligibleUpdateCandidate(
                    stable,
                    nextRc),
                "stable channel rejects future RC");

            Check(
                stable is not null &&
                nextStable is not null &&
                LinuxUpdateService.IsEligibleUpdateCandidate(
                    stable,
                    nextStable),
                "stable channel accepts future stable");

            Check(
                rc1 is not null &&
                stable is not null &&
                LinuxUpdateService.IsEligibleUpdateCandidate(
                    rc1,
                    stable),
                "RC channel can promote to stable");

            Check(
                LinuxUpdateService.ParseTag(
                    "v1.4.0") is null,
                "non-Linux tag rejected");

            var customSettings =
                new LinuxSettings
                {
                    ApplicationTheme = "Dark",
                    Language = "English",
                    ToastTheme = "Custom",
                    ToastFontFamily = "Monospace",
                    TitleFontSize = 18,
                    ArtistFontSize = 13,
                    TimeFontSize = 11,
                    CustomTopColor = "#123456",
                    CustomBottomColor = "#010203",
                    CustomBorderColor = "#333333",
                    CustomTitleColor = "#FFFFFF",
                    CustomSecondaryColor = "#CCCCCC",
                    CustomProgressBackgroundColor = "#222222",
                    CustomProgressForegroundColor = "#00FF88",
                    MonitorIndex = 2,
                    ToastPosition = "TopLeft",
                    ToastMarginX = 31,
                    ToastMarginY = 42,
                    AutoInstallLinuxUpdates = true
                };

            var palette =
                ToastThemePalette.FromSettings(
                    customSettings);

            Check(
                palette.Top == "#123456" &&
                palette.ProgressForeground == "#00FF88",
                "custom theme palette");

            var localization =
                new LocalizationService();

            Check(
                !string.IsNullOrWhiteSpace(
                    localization.Get(
                        "UpdateAvailable",
                        "English")),
                "English localization");

            Check(
                !string.IsNullOrWhiteSpace(
                    localization.Get(
                        "UpdateAvailable",
                        "Italiano")),
                "Italian localization");

            var settingsService =
                new LinuxSettingsService();

            await settingsService.SaveAsync(
                customSettings);

            var reloaded =
                await settingsService.LoadAsync();

            Check(
                reloaded.ToastTheme == "Custom" &&
                reloaded.MonitorIndex == 2 &&
                reloaded.ToastPosition == "TopLeft" &&
                reloaded.AutoInstallLinuxUpdates,
                "settings save/load roundtrip");

            await using var exportStream =
                new MemoryStream();

            await settingsService.ExportAsync(
                exportStream,
                reloaded);

            exportStream.Position = 0;

            var imported =
                await settingsService.ImportAsync(
                    exportStream);

            Check(
                imported.CustomTopColor == "#123456" &&
                imported.ToastFontFamily == "Monospace",
                "settings export/import roundtrip");

            var process =
                new ProcessService();

            var playerctl =
                new PlayerctlService(process);

            Check(
                await playerctl.IsAvailableAsync(),
                "playerctl available in CI/runtime");

            var spicetify =
                new SpicetifyLinuxService(process);

            var spicetifyVersion =
                await spicetify.GetVersionAsync();

            Check(
                !string.IsNullOrWhiteSpace(
                    spicetifyVersion),
                "Spicetify probe is crash-safe");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            failures.Add(
                "unhandled self-test exception");
        }

        Console.WriteLine();
        Console.WriteLine(
            failures.Count == 0
                ? "SELF-TEST RESULT: PASS"
                : $"SELF-TEST RESULT: FAIL ({failures.Count})");

        return failures.Count == 0
            ? 0
            : 1;
    }
}
