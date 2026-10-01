

namespace ATS.Application.Security
{
    public sealed record LetterStyleInfo
    (
        string Text,
        double PointSize,
        double Red,
        double Green,
        double Blue,
        double Left,
        double Right,
        double Bottom,
        double Top,
        bool HasColor = true
    );
}
