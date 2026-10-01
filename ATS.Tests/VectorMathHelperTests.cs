using ATS.Core.Helpers;

namespace ATS.Tests
{
    public class VectorMathHelperTests
    {
        [Fact]
        public void CosineSimilarity_IdenticalVectors_ReturnsOne()
        {
            var result = VectorMathHelper.CosineSimilarity(new[] { 1f, 2f, 3f }, new[] { 1f, 2f, 3f });

            Assert.Equal(1.0, result, 6);
        }

        [Fact]
        public void CosineSimilarity_OrthogonalVectors_ReturnsZero()
        {
            var result = VectorMathHelper.CosineSimilarity(new[] { 1f, 0f }, new[] { 0f, 1f });

            Assert.Equal(0.0, result, 6);
        }

        [Fact]
        public void CosineSimilarity_OppositeVectors_ReturnsMinusOne()
        {
            var result = VectorMathHelper.CosineSimilarity(new[] { 1f, 2f }, new[] { -1f, -2f });

            Assert.Equal(-1.0, result, 6);
        }

        [Fact]
        public void CosineSimilarity_IgnoresVectorMagnitude()
        {
            var result = VectorMathHelper.CosineSimilarity(new[] { 1f, 2f, 3f }, new[] { 10f, 20f, 30f });

            Assert.Equal(1.0, result, 6);
        }

        [Fact]
        public void CosineSimilarity_ReturnsZero_WhenEitherVectorIsZero()
        {
            var result = VectorMathHelper.CosineSimilarity(new[] { 0f, 0f }, new[] { 1f, 2f });

            Assert.Equal(0.0, result);
        }

        [Fact]
        public void CosineSimilarity_Throws_WhenLengthsDiffer()
        {
            Assert.Throws<ArgumentException>(() =>
                VectorMathHelper.CosineSimilarity(new[] { 1f, 2f }, new[] { 1f, 2f, 3f }));
        }
    }
}
