using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ToastifyReloaded.Mac.Services;

public sealed class ArtworkService
{
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    public async Task<Bitmap?> LoadAsync(string artworkUrl, bool fallback)
    {
        if (!string.IsNullOrWhiteSpace(artworkUrl))
        {
            try
            {
                var uri = new Uri(artworkUrl);

                if (uri.IsFile)
                    return new Bitmap(uri.LocalPath);

                if (uri.Scheme is "http" or "https")
                {
                    await using var stream = await Http.GetStreamAsync(uri);
                    using var memory = new MemoryStream();
                    await stream.CopyToAsync(memory);
                    memory.Position = 0;
                    return new Bitmap(memory);
                }
            }
            catch
            {
                // Fall back below.
            }
        }

        if (!fallback)
            return null;

        try
        {
            await using var stream = AssetLoader.Open(
                new Uri("avares://ToastifyReloaded.Mac/Assets/toastify.png"));
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }
}
