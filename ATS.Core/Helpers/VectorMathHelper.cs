

namespace ATS.Core.Helpers
{
    public static class VectorMathHelper
    {
        public static double CosineSimilarity(ReadOnlySpan<float> left, ReadOnlySpan<float> right)
        {
            if (left.Length != right.Length)
                throw new ArgumentException("Vectors must have the same length.");

            double dotProduct = 0;
            double leftNormSquared = 0;
            double rightNormSquared = 0;

            for (int index = 0; index < left.Length; index++)
            {
                dotProduct += left[index] * right[index];
                leftNormSquared += left[index] * left[index];
                rightNormSquared += right[index] * right[index];
            }

            if (leftNormSquared == 0 || rightNormSquared == 0)
                return 0;

            return dotProduct / (Math.Sqrt(leftNormSquared) * Math.Sqrt(rightNormSquared));
        }
    }
}
