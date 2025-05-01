using Microsoft.AspNetCore.Http;
using ModelLayer.Entity;
using RepoLayer.DTO;

namespace BusinessLayer.Interface
{
    public interface INotesBL
    {
        /// <summary>
        /// Creates a new note for the specified user.
        /// </summary>
        /// <param name="request">The note creation details.</param>
        /// <param name="userId">The ID of the user creating the note.</param>
        /// <returns>A response containing the created note entity.</returns>
        Task<ResponseDTO<NotesEntity>> CreateNotesAsync(CreateNotesDTO request, int userId);

        /// <summary>
        /// Retrieves all notes belonging to the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user whose notes are to be retrieved.</param>
        /// <returns>A response containing a list of note entities.</returns>
        Task<ResponseDTO<List<NotesEntity>>> GetAllNotesAsync(int userId);

        /// <summary>
        /// Deletes a note based on its title.
        /// </summary>
        /// <param name="title">The title of the note to delete.</param>
        /// <returns>A response containing a success or failure message.</returns>
        Task<ResponseDTO<string>> DeleteNoteAsync(string title);

        /// <summary>
        /// Updates an existing note's details.
        /// </summary>
        /// <param name="request">The updated note details.</param>
        /// <param name="noteId">The ID of the note to update.</param>
        /// <returns>A response containing the updated note entity.</returns>
        Task<ResponseDTO<NotesEntity>> UpdateNoteAsync(UpdateNotesDTO request, int noteId);

        /// <summary>
        /// Pins or unpins a note based on its title and ID.
        /// </summary>
        /// <param name="title">The title of the note to pin/unpin.</param>
        /// <param name="noteId">The ID of the note to pin/unpin.</param>
        /// <returns>A response indicating whether the operation was successful.</returns>
        Task<ResponseDTO<bool>> PinUnpinNoteAsync(string title, int noteId);

        /// <summary>
        /// Archives a specified note.
        /// </summary>
        /// <param name="noteId">The ID of the note to archive.</param>
        /// <param name="userId">The ID of the user archiving the note.</param>
        /// <returns>A response indicating whether the operation was successful.</returns>
        Task<ResponseDTO<bool>> ArchiveNoteAsync(int noteId, int userId);

        /// <summary>
        /// Unarchives a specified note.
        /// </summary>
        /// <param name="noteId">The ID of the note to unarchive.</param>
        /// <param name="userId">The ID of the user unarchiving the note.</param>
        /// <returns>A response indicating whether the operation was successful.</returns>
        Task<ResponseDTO<bool>> UnArchiveNoteAsync(int noteId, int userId);

        /// <summary>
        /// Changes the background color of a specified note.
        /// </summary>
        /// <param name="noteId">The ID of the note to update.</param>
        /// <param name="backgroundColor">The new background color to set.</param>
        /// <returns>A response containing a success or failure message.</returns>
        Task<ResponseDTO<string>> BackgroundColorNoteAsync(int noteId, string backgroundColor);

        /// <summary>
        /// Moves a specified note to the trash.
        /// </summary>
        /// <param name="title">The title of the note to trash.</param>
        /// <param name="noteId">The ID of the note to trash.</param>
        /// <returns>A response indicating whether the operation was successful.</returns>
        Task<ResponseDTO<bool>> TrashNoteAsync(int noteId, int userId);

        /// <summary>
        /// Restores a trashed note back to active notes.
        /// </summary>
        /// <param name="noteId">The ID of the note to restore.</param>
        /// <param name="userId">The ID of the user restoring the note.</param>
        /// <returns>A response containing the restored note entity.</returns>
        Task<ResponseDTO<NotesEntity>> RestoreNoteAsync(int noteId, int userId);
        /// <summary>
        /// Uploads a image for specific note.
        /// </summary>
        /// <param name="noteId"> The specific note for which image is to be uploaded.</param>
        /// <param name="userId"> The ID of the user uploading the image.</param>
        /// <param name="imageFile"> The image file to be uploaded.</param>
        Task<ResponseDTO<string>> UploadImageAsync(int noteId, int userId, IFormFile imageFile);
    }
}
