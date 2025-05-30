using BusinessLayer.Interface;
using Microsoft.AspNetCore.Http;
using ModelLayer.Entity;
using RepoLayer.DTO;
using RepoLayer.Interface;

namespace BusinessLayer.Service
{
    public class NotesImplBL : INotesBL
    {
        public INotesRL _notesRL;
        public NotesImplBL(INotesRL notesRL)
        {
            _notesRL = notesRL;
        }
        public async Task<ResponseDTO<NoteEntity>> CreateNotesAsync(CreateNotesDTO notesEntity, int userId)
        {
            try
            {
                return await _notesRL.CreateNotesAsync(notesEntity, userId);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<NoteEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseDTO<List<NoteEntity>>> GetAllNotesAsync(int noteId)
        {
            try
            {
                return await _notesRL.GetAllNotesAsync(noteId);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<List<NoteEntity>>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseDTO<string>> DeleteNoteAsync(string title)
        {
            try
            {
                return await _notesRL.DeleteNoteAsync(title);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseDTO<NoteEntity>> UpdateNoteAsync(int noteId, int userId, UpdateNotesDTO request)
        {
            try
            {
                return await _notesRL.UpdateNoteAsync(noteId, userId, request);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<NoteEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseDTO<bool>> PinUnpinNoteAsync(int noteId, int userId)
        {
            try
            {
                return await _notesRL.PinUnpinNoteAsync(noteId, userId);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseDTO<string>> BackgroundColorNoteAsync(int userId, int noteId, string backgroundColor)
        {
            try
            {
                return await _notesRL.BackgroundColorNoteAsync(userId, noteId, backgroundColor);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseDTO<bool>> TrashNoteAsync(int noteId, int userId)
        {
            try
            {
                return await _notesRL.TrashNoteAsync(noteId, userId);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseDTO<NoteEntity>> RestoreNoteAsync(int noteId, int userId)
        {
            try
            {
                return await _notesRL.RestoreNoteAsync(noteId, userId);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<NoteEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO<bool>> ArchiveNoteAsync(int noteId, int userId)
        {
            try
            {
                return await _notesRL.ArchiveNoteAsync(noteId, userId);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseDTO<bool>> UnArchiveNoteAsync(int noteId, int userId)
        {
            try
            {
                return await _notesRL.UnArchiveNoteAsync(noteId, userId);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseDTO<string>> UploadImageAsync(int noteId, int userId, IFormFile imageFile)
        {
            try
            {
                return await _notesRL.UploadImageAsync(noteId, userId, imageFile);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}
