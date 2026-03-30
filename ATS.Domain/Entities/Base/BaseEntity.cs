using System.ComponentModel.DataAnnotations;

namespace ATS.Domain.Entities.Base
{
    public class BaseEntity : IEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
