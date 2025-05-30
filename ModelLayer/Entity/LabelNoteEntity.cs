using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ModelLayer.Entity
{
    public class LabelNoteEntity
    {
        [Key]
        public int LabelNoteId { get; set; }

        [ForeignKey("Label")]
        public int LabelId { get; set; }
        public LabelEntity Label { get; set; }

        [ForeignKey("Note")]
        public int NoteId { get; set; }
        [JsonIgnore]
        public NoteEntity Note { get; set; }
    }
}
