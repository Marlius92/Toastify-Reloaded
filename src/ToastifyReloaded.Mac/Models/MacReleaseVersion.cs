namespace ToastifyReloaded.Mac.Models;

public enum MacReleaseStage
{
    Preview = 0,
    ReleaseCandidate = 1,
    Stable = 2
}

public sealed record MacReleaseVersion(
    int Major,
    int Minor,
    int Patch,
    MacReleaseStage Stage,
    int StageNumber,
    string Tag)
    : IComparable<MacReleaseVersion>
{
    public int CompareTo(MacReleaseVersion? other)
    {
        if (other is null)
            return 1;

        var result = Major.CompareTo(other.Major);
        if (result != 0) return result;

        result = Minor.CompareTo(other.Minor);
        if (result != 0) return result;

        result = Patch.CompareTo(other.Patch);
        if (result != 0) return result;

        result = Stage.CompareTo(other.Stage);
        if (result != 0) return result;

        return StageNumber.CompareTo(other.StageNumber);
    }
}
