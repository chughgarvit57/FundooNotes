using Microsoft.AspNetCore.Http;
using ModelLayer.Entity;
using RepoLayer.DTO;

namespace RepoLayer.Interface
{
    public interface INotesRL
    {
        Task<ResponseDTO<NoteEntity>> CreateNotesAsync(CreateNotesDTO request, int userId);
        Task<ResponseDTO<List<NoteEntity>>> GetAllNotesAsync(int userId);
        Task<ResponseDTO<string>> DeleteNoteAsync(string title);
        Task<ResponseDTO<NoteEntity>> UpdateNoteAsync(int noteId, int userId, UpdateNotesDTO request);
        Task<ResponseDTO<bool>> PinUnpinNoteAsync(int noteId, int userId);
        Task<ResponseDTO<bool>> ArchiveNoteAsync(int noteId, int userId);
        Task<ResponseDTO<bool>> UnArchiveNoteAsync(int noteId, int userId);
        Task<ResponseDTO<string>> BackgroundColorNoteAsync(int userId, int noteId, string backgroundColor);
        Task<ResponseDTO<bool>> TrashNoteAsync(int noteId, int userId);
        Task<ResponseDTO<NoteEntity>> RestoreNoteAsync(int noteId, int userId);
        Task<ResponseDTO<string>> UploadImageAsync(int noteId, int userId, IFormFile imageFile);
    }
}
