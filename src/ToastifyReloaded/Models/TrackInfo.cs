namespace ToastifyReloaded.Models;

public sealed record TrackInfo(string Title, string Artist, string Album, byte[]? ArtworkBytes = null)
{
    public string Identity => $"{Title}\u001f{Artist}\u001f{Album}";

    public static TrackInfo Empty { get; } = new("Nessun brano", "Spotify non rilevato", string.Empty);
}
