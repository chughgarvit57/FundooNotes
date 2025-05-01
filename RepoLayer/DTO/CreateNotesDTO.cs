using System.ComponentModel.DataAnnotations;

namespace RepoLayer.DTO
{
    public class CreateNotesDTO
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        public DateTime Reminder { get; set; }
        [Required(ErrorMessage = "Background color is required")]
        public string BackgroundColor { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public bool Pin { get; set; }
        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public bool Trash { get; set; }
        public bool Archive { get; set; }
    }
}
