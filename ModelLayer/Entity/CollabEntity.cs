using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ModelLayer.Entity
{
    public class CollabEntity
    {
        [Key]
        public int CollabId { get; set; }
        [ForeignKey("Users")]
        public int UserId { get; set; }
        [ForeignKey("Notes")]
        public int NoteId { get; set; }
        public string CollabEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [JsonIgnore]
        public UserEntity Users { get; set; }
        [JsonIgnore]
        public NoteEntity Notes { get; set; }
    }
}
