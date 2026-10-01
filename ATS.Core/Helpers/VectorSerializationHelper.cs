using System.Runtime.InteropServices;

namespace ATS.Core.Helpers
{
    public static class VectorSerializationHelper
    {
        public static byte[] ToBytes(ReadOnlySpan<float> vector)
        {
            return MemoryMarshal.AsBytes(vector).ToArray();
        }

        public static float[] ToVector(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length % sizeof(float) != 0)
                throw new ArgumentException("Byte length must be a multiple of four.", nameof(bytes));

            return MemoryMarshal.Cast<byte, float>(bytes).ToArray();
        }
    }
}
