using System.Runtime.InteropServices;
using System.Text;
using ToastifyReloaded.Mac.Models;

namespace ToastifyReloaded.Mac.Services;

public static class MacSelfTestService
{
    public static async Task<int> RunAsync()
    {
        var failures = new List<string>();

        void Check(bool condition, string name)
        {
            if (condition)
                Console.WriteLine($"PASS: {name}");
            else
            {
                Console.Error.WriteLine($"FAIL: {name}");
                failures.Add(name);
            }
        }

        var oldConfigOverride = Environment.GetEnvironmentVariable("TOASTIFY_RELOADED_CONFIG_DIR");
        var tempRoot = Path.Combine(Path.GetTempPath(), "toastify-macos-selftest-" + Guid.NewGuid().ToString("N"));

        try
        {
            Environment.SetEnvironmentVariable("TOASTIFY_RELOADED_CONFIG_DIR", tempRoot);

            Console.WriteLine("Toastify Reloaded macOS 1.5.0 Preview 1 self-test");
            Console.WriteLine($"Runtime: {Environment.Version}");
            Console.WriteLine($"OS: {Environment.OSVersion}");
            Console.WriteLine($"Architecture: {RuntimeInformation.ProcessArchitecture}");

            Check(
                MacUpdateService.CurrentTag == "v1.5.0-macos-preview.1",
                "preview version constant");

            var preview1 = MacUpdateService.ParseTag("v1.5.0-macos-preview.1");
            var preview2 = MacUpdateService.ParseTag("v1.5.0-macos-preview.2");
            var rc1 = MacUpdateService.ParseTag("v1.5.0-macos-rc.1");
            var stable = MacUpdateService.ParseTag("v1.5.0-macos");
            var nextPreview = MacUpdateService.ParseTag("v1.5.1-macos-preview.1");
            var nextStable = MacUpdateService.ParseTag("v1.5.1-macos");

            Check(preview1 is not null && preview2 is not null && preview1.CompareTo(preview2) < 0,
                "preview versions sort correctly");
            Check(preview2 is not null && rc1 is not null && preview2.CompareTo(rc1) < 0,
                "preview sorts before RC");
            Check(rc1 is not null && stable is not null && rc1.CompareTo(stable) < 0,
                "RC sorts before stable");
            Check(stable is not null && nextStable is not null && stable.CompareTo(nextStable) < 0,
                "semantic stable versions sort correctly");
            Check(stable is not null && nextPreview is not null &&
                  !MacUpdateService.IsEligibleUpdateCandidate(stable, nextPreview),
                "stable channel rejects future preview");
            Check(stable is not null && nextStable is not null &&
                  MacUpdateService.IsEligibleUpdateCandidate(stable, nextStable),
                "stable channel accepts future stable");
            Check(rc1 is not null && stable is not null &&
                  MacUpdateService.IsEligibleUpdateCandidate(rc1, stable),
                "RC can promote to stable");
            Check(MacUpdateService.ParseTag("v1.5.0-linux") is null &&
                  MacUpdateService.ParseTag("v1.5.0") is null,
                "non-macOS tags rejected");

            var customSettings = new MacSettings
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
                EnableGlobalHotkeys = true,
                AutoInstallMacUpdates = true
            };

            var palette = ToastThemePalette.FromSettings(customSettings);
            Check(palette.Top == "#123456" && palette.ProgressForeground == "#00FF88",
                "custom theme palette");

            var normalized = MacSettingsService.Normalize(new MacSettings
            {
                MinWidth = 700,
                MaxWidth = 300,
                FadeInMs = -20,
                ToastMarginX = 800,
                CustomTopColor = "not-a-color"
            });
            Check(normalized.MaxWidth >= normalized.MinWidth &&
                  normalized.FadeInMs == 0 &&
                  normalized.ToastMarginX == 500 &&
                  normalized.CustomTopColor == "#555555",
                "settings normalization");

            var settingsService = new MacSettingsService();
            await settingsService.SaveAsync(customSettings);
            var reloaded = await settingsService.LoadAsync();
            Check(reloaded.ToastTheme == "Custom" &&
                  reloaded.MonitorIndex == 2 &&
                  reloaded.ToastPosition == "TopLeft" &&
                  reloaded.AutoInstallMacUpdates,
                "settings save/load roundtrip");

            await using var exportStream = new MemoryStream();
            await settingsService.ExportAsync(exportStream, reloaded);
            exportStream.Position = 0;
            var imported = await settingsService.ImportAsync(exportStream);
            Check(imported.CustomTopColor == "#123456" && imported.ToastFontFamily == "Monospace",
                "settings export/import roundtrip");

            const string linuxSettingsJson = """
            {
              "ApplicationTheme": "Dark",
              "Language": "English",
              "EnableX11GlobalHotkeys": true,
              "EnableWaylandPortalHotkeys": false,
              "AutoCheckLinuxUpdates": false,
              "AutoInstallLinuxUpdates": true,
              "ToastTheme": "Sakura",
              "HotkeyPlayPause": "Ctrl+Alt+Space"
            }
            """;
            await using var linuxSettingsStream = new MemoryStream(Encoding.UTF8.GetBytes(linuxSettingsJson));
            var importedLinux = await settingsService.ImportAsync(linuxSettingsStream);
            Check(importedLinux.EnableGlobalHotkeys &&
                  !importedLinux.AutoCheckMacUpdates &&
                  importedLinux.AutoInstallMacUpdates &&
                  importedLinux.ToastTheme == "Sakura",
                "Linux settings import compatibility");

            var payload = string.Join('\u001f', new[]
            {
                "Get Lucky",
                "Daft Punk",
                "Random Access Memories",
                "360000",
                "76.5",
                "playing",
                "spotify:image:abc123"
            });
            var track = SpotifyAppleScriptService.ParseTrackPayload(payload);
            Check(track is not null &&
                  track.Title == "Get Lucky" &&
                  Math.Abs(track.DurationSeconds - 360) < 0.01 &&
                  Math.Abs(track.PositionSeconds - 76.5) < 0.01 &&
                  track.IsPlaying &&
                  track.ArtworkUrl == "https://i.scdn.co/image/abc123",
                "Spotify AppleScript metadata parser");

            Check(MacGlobalHotkeyService.ParseShortcut("Ctrl+Alt+Space") is not null &&
                  MacGlobalHotkeyService.ParseShortcut("Cmd+Shift+Right") is not null &&
                  MacGlobalHotkeyService.ParseShortcut("Ctrl+Alt+Shift+Left") is not null &&
                  MacGlobalHotkeyService.ParseShortcut("Ctrl+Bogus+Space") is null,
                "global hotkey parser");

            var ctrlAlt = MacGlobalHotkeyService.ParseShortcut("Ctrl+Alt+Space");
            var normalizedCtrlAlt = MacGlobalHotkeyService.NormalizeModifiers(
                SharpHook.Data.EventMask.LeftCtrl | SharpHook.Data.EventMask.LeftAlt);
            Check(ctrlAlt is not null && ctrlAlt.Value.Modifiers == normalizedCtrlAlt,
                "global hotkey left/right modifier normalization");

            var release = new MacUpdateInfo(
                stable ?? throw new InvalidOperationException("stable test tag parse failed"),
                new Uri("https://github.com/Marlius92/Toastify-Reloaded/releases/tag/v1.5.0-macos"),
                new MacUpdateAsset[]
                {
                    new("ToastifyReloaded-macOS-arm64.zip", new Uri("https://example.invalid/arm64.zip"), 1, null),
                    new("ToastifyReloaded-macOS-x64.zip", new Uri("https://example.invalid/x64.zip"), 1, null)
                });

            Check(MacUpdateInstallerService.SelectAsset(release, Architecture.Arm64)?.Name.EndsWith("arm64.zip", StringComparison.Ordinal) == true &&
                  MacUpdateInstallerService.SelectAsset(release, Architecture.X64)?.Name.EndsWith("x64.zip", StringComparison.Ordinal) == true,
                "architecture-specific updater assets");

            Check(MacSpotifyVersionService.ExtractVersion("Spotify 1.2.99.1234") == "1.2.99.1234",
                "Spotify bundle version parser");

            var localization = new LocalizationService();
            Check(localization.Get("Updates", "English") == "macOS updates" &&
                  localization.Get("Updates", "Italiano") == "Aggiornamenti macOS",
                "English/Italian localization");

            if (OperatingSystem.IsMacOS())
            {
                var process = new ProcessService();
                var spotify = new SpotifyAppleScriptService(process);
                Check(await spotify.IsAvailableAsync(), "osascript available on macOS runner");
            }
            else
            {
                Console.WriteLine("SKIP: osascript runtime probe (not running on macOS)");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            failures.Add("unhandled self-test exception");
        }
        finally
        {
            Environment.SetEnvironmentVariable("TOASTIFY_RELOADED_CONFIG_DIR", oldConfigOverride);
            try
            {
                if (Directory.Exists(tempRoot))
                    Directory.Delete(tempRoot, recursive: true);
            }
            catch
            {
            }
        }

        Console.WriteLine();
        Console.WriteLine(failures.Count == 0
            ? "SELF-TEST RESULT: PASS"
            : $"SELF-TEST RESULT: FAIL ({failures.Count})");

        return failures.Count == 0 ? 0 : 1;
    }
}
