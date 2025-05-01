namespace RepoLayer.DTO
{
    public class PinStatusDTO
    {
        public int NoteId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsPinned { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
