namespace ToastifyReloaded.Linux.Models;

public enum LinuxReleaseStage
{
    Preview = 0,
    ReleaseCandidate = 1,
    Stable = 2
}

public sealed record LinuxReleaseVersion(
    int Major,
    int Minor,
    int Patch,
    LinuxReleaseStage Stage,
    int StageNumber,
    string Tag)
    : IComparable<LinuxReleaseVersion>
{
    public int CompareTo(LinuxReleaseVersion? other)
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
