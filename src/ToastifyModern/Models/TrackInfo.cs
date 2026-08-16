namespace ToastifyModern.Models;

public sealed record TrackInfo(string Title, string Artist, string Album)
{
    public string Identity => $"{Title}\u001f{Artist}\u001f{Album}";

    public static TrackInfo Empty { get; } = new("Nessun brano", "Spotify non rilevato", string.Empty);
}
