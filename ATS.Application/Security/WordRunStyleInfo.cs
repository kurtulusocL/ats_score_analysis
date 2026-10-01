

namespace ATS.Application.Security
{
    public sealed record WordRunStyleInfo(
        string Text,
        bool IsVanished,
        double? PointSize,
        string? FontColorHex,
        string? BackgroundFillHex);
}
