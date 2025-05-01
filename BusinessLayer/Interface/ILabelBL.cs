using ModelLayer.Entity;
using RepoLayer.DTO;

namespace BusinessLayer.Interface
{
    public interface ILabelBL
    {
        /// <summary>
        /// Creates a new label for the specified user.
        /// </summary>
        /// <param name="labelName">The name of the label to create.</param>
        /// <param name="userId">The ID of the user creating the label.</param>
        /// <returns>A response containing the created label entity.</returns>
        Task<ResponseDTO<LabelEntity>> CreateLabelAsync(string labelName, int userId);

        /// <summary>
        /// Deletes an existing label for the specified user.
        /// </summary>
        /// <param name="labelName">The name of the label to delete.</param>
        /// <param name="userId">The ID of the user deleting the label.</param>
        /// <returns>A response containing the deleted label entity.</returns>
        Task<ResponseDTO<LabelEntity>> DeleteLabelAsync(string labelName, int userId);

        /// <summary>
        /// Associates an existing label with a specified note.
        /// </summary>
        /// <param name="labelName">The name of the label to associate.</param>
        /// <param name="noteId">The ID of the note to which the label is added.</param>
        /// <param name="userId">The ID of the user performing the association.</param>
        /// <returns>A response containing the label entity associated with the note.</returns>
        Task<ResponseDTO<LabelEntity>> AddLabelToNoteAsync(string labelName, int noteId, int userId);

        /// <summary>
        /// Retrieves all labels created by the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user whose labels are to be retrieved.</param>
        /// <returns>A response containing a list of label entities.</returns>
        Task<ResponseDTO<List<LabelEntity>>> ViewAllLabelsAsync(int userId);

        /// <summary>
        /// Updates an existing label's information for the specified user.
        /// </summary>
        /// <param name="request">The updated label data.</param>
        /// <param name="userId">The ID of the user updating the label.</param>
        /// <returns>A response containing the updated label entity.</returns>
        Task<ResponseDTO<LabelEntity>> UpdateLabelAsync(LabelUpdateDTO request, int userId);

        /// <summary>
        /// Retrieves a label by its ID for the specified user.
        /// </summary>
        /// <param name="labelId">The ID of the label to retrieve.</param>
        /// <param name="userId">The ID of the user retrieving the label.</param>
        /// <returns>A response containing the requested label entity.</returns>
        Task<ResponseDTO<LabelEntity>> GetLabelByIdAsync(int labelId, int userId);
    }
}
