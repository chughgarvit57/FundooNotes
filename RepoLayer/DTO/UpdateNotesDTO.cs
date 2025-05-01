namespace RepoLayer.DTO
{
    public class UpdateNotesDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BackgroundColor { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public DateTime Reminder { get; set; }
        public bool Pin { get; set; }
        public bool Trash { get; set; }
        public bool Archive { get; set; }
        public DateTime Edited { get; set; }
    }
}
