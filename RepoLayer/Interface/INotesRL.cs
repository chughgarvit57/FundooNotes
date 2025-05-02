using Microsoft.AspNetCore.Http;
using ModelLayer.Entity;
using RepoLayer.DTO;

namespace RepoLayer.Interface
{
    public interface INotesRL
    {
        Task<ResponseDTO<NotesEntity>> CreateNotesAsync(CreateNotesDTO request, int userId);
        Task<ResponseDTO<List<NotesEntity>>> GetAllNotesAsync(int userId);
        Task<ResponseDTO<string>> DeleteNoteAsync(string title);
        Task<ResponseDTO<NotesEntity>> UpdateNoteAsync(UpdateNotesDTO request, int noteId);
        Task<ResponseDTO<bool>> PinUnpinNoteAsync(string title, int noteId);
        Task<ResponseDTO<bool>> ArchiveNoteAsync(int noteId, int userId);
        Task<ResponseDTO<bool>> UnArchiveNoteAsync(int noteId, int userId);
        Task<ResponseDTO<string>> BackgroundColorNoteAsync(int noteId, string backgroundColor);
        Task<ResponseDTO<bool>> TrashNoteAsync(int noteId, int userId);
        Task<ResponseDTO<NotesEntity>> RestoreNoteAsync(int noteId, int userId);
        Task<ResponseDTO<string>> UploadImageAsync(int noteId, int userId, IFormFile imageFile);
    }
}
