using Microsoft.AspNetCore.Http;
using ModelLayer.Entity;
using RepoLayer.DTO;

namespace RepoLayer.Interface
{
    public interface INotesRL
    {
        /// <summary>
        /// Creates a new note for a user.
        /// </summary>
        /// <param name="request">The request containing details for the new note.</param>
        /// <param name="userId">The ID of the user creating the note.</param>
        /// <returns>Returns a response containing the created note entity.</returns>
        Task<ResponseDTO<NotesEntity>> CreateNotesAsync(CreateNotesDTO request, int userId);

        /// <summary>
        /// Retrieves all notes for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose notes are to be retrieved.</param>
        /// <returns>Returns a list of note entities belonging to the specified user.</returns>
        Task<ResponseDTO<List<NotesEntity>>> GetAllNotesAsync(int userId);

        /// <summary>
        /// Deletes a note by its title.
        /// </summary>
        /// <param name="title">The title of the note to be deleted.</param>
        /// <returns>Returns a response containing a message indicating the result of the deletion.</returns>
        Task<ResponseDTO<string>> DeleteNoteAsync(string title);

        /// <summary>
        /// Updates the details of an existing note.
        /// </summary>
        /// <param name="request">The request containing updated note details.</param>
        /// <param name="noteId">The ID of the note to be updated.</param>
        /// <returns>Returns a response containing the updated note entity.</returns>
        Task<ResponseDTO<NotesEntity>> UpdateNoteAsync(UpdateNotesDTO request, int noteId);

        /// <summary>
        /// Pins or unpins a note.
        /// </summary>
        /// <param name="title">The title of the note to be pinned or unpinned.</param>
        /// <param name="noteId">The ID of the note to be pinned or unpinned.</param>
        /// <returns>Returns a response indicating whether the note was successfully pinned or unpinned.</returns>
        Task<ResponseDTO<bool>> PinUnpinNoteAsync(string title, int noteId);

        /// <summary>
        /// Archives a note.
        /// </summary>
        /// <param name="noteId">The ID of the note to be archived.</param>
        /// <param name="userId">The ID of the user archiving the note.</param>
        /// <returns>Returns a response indicating the success of the archive operation.</returns>
        Task<ResponseDTO<bool>> ArchiveNoteAsync(int noteId, int userId);

        /// <summary>
        /// Unarchives a note.
        /// </summary>
        /// <param name="noteId">The ID of the note to be unarchived.</param>
        /// <param name="userId">The ID of the user unarchiving the note.</param>
        /// <returns>Returns a response indicating the success of the unarchive operation.</returns>
        Task<ResponseDTO<bool>> UnArchiveNoteAsync(int noteId, int userId);

        /// <summary>
        /// Changes the background color of a note.
        /// </summary>
        /// <param name="noteId">The ID of the note whose background color is to be changed.</param>
        /// <param name="backgroundColor">The new background color to be set for the note.</param>
        /// <returns>Returns a response indicating whether the background color change was successful.</returns>
        Task<ResponseDTO<string>> BackgroundColorNoteAsync(int noteId, string backgroundColor);

        /// <summary>
        /// Moves a note to the trash.
        /// </summary>
        /// <param name="title">The title of the note to be moved to trash.</param>
        /// <param name="noteId">The ID of the note to be trashed.</param>
        /// <returns>Returns a response indicating whether the note was successfully moved to the trash.</returns>
        Task<ResponseDTO<bool>> TrashNoteAsync(int noteId, int userId);

        /// <summary>
        /// Restores a note from the trash.
        /// </summary>
        /// <param name="noteId">The ID of the note to be restored.</param>
        /// <param name="userId">The ID of the user restoring the note.</param>
        /// <returns>Returns a response containing the restored note entity.</returns>
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
