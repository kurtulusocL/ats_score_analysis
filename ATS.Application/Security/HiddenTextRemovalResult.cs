

namespace ATS.Application.Security
{
    public sealed record HiddenTextRemovalResult(string Text, int RemovedCount, int NotFoundCount);
}
