// DevUnlockService.cs — Checks for the presence of %LocalAppData%\RHI\unlock.txt
// to gate developer-only features. Result is cached for the lifetime of the process.

namespace RenoDXCommander.Services;

/// <summary>
/// Gates developer-only features behind the existence of %LocalAppData%\RHI\unlock.txt.
/// The check is performed once and cached — restart required to pick up changes.
/// </summary>
public static class DevUnlockService
{
    private static readonly string UnlockFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "RHI", "unlock.txt");

    private static bool? _isUnlocked;

    /// <summary>
    /// Returns true if %LocalAppData%\RHI\unlock.txt exists.
    /// Result is cached after the first call.
    /// </summary>
    public static bool IsUnlocked => _isUnlocked ??= File.Exists(UnlockFilePath);

    private static string? _gitHubApiToken;
    private static bool _gitHubApiTokenRead;

    /// <summary>
    /// Returns a GitHub API token read from unlock.txt via a "github_api=TOKEN" line.
    /// Only available when unlock.txt exists. Returns null if not set.
    /// </summary>
    public static string? GitHubApiToken
    {
        get
        {
            if (_gitHubApiTokenRead) return _gitHubApiToken;
            _gitHubApiTokenRead = true;
            if (!IsUnlocked) return null;
            try
            {
                foreach (var line in File.ReadLines(UnlockFilePath))
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("github_api=", StringComparison.OrdinalIgnoreCase))
                    {
                        _gitHubApiToken = trimmed.Substring("github_api=".Length).Trim();
                        return _gitHubApiToken;
                    }
                }
            }
            catch { }
            return null;
        }
    }
}
