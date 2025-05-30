using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ModelLayer.Entity
{
    public class NoteEntity
    {
        [Key]
        public int NoteId { get; set; }

        [ForeignKey("Users")]
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Reminder { get; set; }
        public string BackgroundColor { get; set; } = "white";
        public string Image { get; set; } = string.Empty;
        public bool Pin { get; set; }
        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public bool Trash { get; set; }
        public bool Archive { get; set; }
        [JsonIgnore]
        public UserEntity User { get; set; }
        [JsonIgnore]
        public ICollection<LabelNoteEntity> LabelNotes { get; set; } = new List<LabelNoteEntity>();
    }
}
