using ATS.Domain.Entities.Base;

namespace ATS.Domain.Entities
{
    public class EmbeddingCacheEntry:BaseEntity
    {
        public string TextHash { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Dimensions { get; set; }
        public byte[] Vector { get; set; } = Array.Empty<byte>();
    }
}
