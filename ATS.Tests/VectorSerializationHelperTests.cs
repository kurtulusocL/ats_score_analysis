using ATS.Core.Helpers;

namespace ATS.Tests
{
    public class VectorSerializationHelperTests
    {
        [Fact]
        public void ToBytes_ThenToVector_ReturnsOriginalValues()
        {
            var original = new[] { 0.5f, -1.25f, 3.0f, 0f };

            var restored = VectorSerializationHelper.ToVector(VectorSerializationHelper.ToBytes(original));

            Assert.Equal(original, restored);
        }

        [Fact]
        public void ToBytes_UsesFourBytesPerValue()
        {
            var bytes = VectorSerializationHelper.ToBytes(new[] { 1f, 2f, 3f });

            Assert.Equal(12, bytes.Length);
        }

        [Fact]
        public void ToVector_Throws_WhenByteLengthIsNotMultipleOfFour()
        {
            Assert.Throws<ArgumentException>(() => VectorSerializationHelper.ToVector(new byte[] { 1, 2, 3 }));
        }
    }
}
