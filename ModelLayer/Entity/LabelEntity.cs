using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ModelLayer.Entity
{
    public class LabelEntity
    {
        [Key]
        public int LabelId { get; set; }
        public string LabelName { get; set; } = string.Empty;
        [ForeignKey("Users")]
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        [JsonIgnore]
        public ICollection<LabelNoteEntity> LabelNotes { get; set; } = new List<LabelNoteEntity>();
        [JsonIgnore]
        public UserEntity Users { get; set; }
    }
}
