using ModelLayer.Entity;
using RepoLayer.DTO;

namespace RepoLayer.Interface
{
    public interface ILabelRL
    {
        /// <summary>
        /// Creates a new label for the user.
        /// </summary>
        /// <param name="labelName">The name of the label to be created.</param>
        /// <param name="userId">The ID of the user creating the label.</param>
        /// <returns>Returns a response containing the created label entity.</returns>
        Task<ResponseDTO<LabelEntity>> CreateLabelAsync(string labelName, int userId);

        /// <summary>
        /// Deletes an existing label for the user.
        /// </summary>
        /// <param name="labelName">The name of the label to be deleted.</param>
        /// <param name="userId">The ID of the user deleting the label.</param>
        /// <returns>Returns a response containing the deleted label entity.</returns>
        Task<ResponseDTO<LabelEntity>> DeleteLabelAsync(string labelName, int userId);

        /// <summary>
        /// Adds a label to a specific note.
        /// </summary>
        /// <param name="labelName">The name of the label to be added.</param>
        /// <param name="noteId">The ID of the note to which the label is being added.</param>
        /// <param name="userId">The ID of the user adding the label.</param>
        /// <returns>Returns a response containing the label entity added to the note.</returns>
        Task<ResponseDTO<LabelEntity>> AddLabelToNoteAsync(string labelName, int noteId, int userId);

        /// <summary>
        /// Views all labels created by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose labels are to be viewed.</param>
        /// <returns>Returns a list of label entities for the specified user.</returns>
        Task<ResponseDTO<List<LabelEntity>>> ViewAllLabelsAsync(int userId);

        /// <summary>
        /// Updates an existing label for the user.
        /// </summary>
        /// <param name="request">The request containing updated label details.</param>
        /// <param name="userId">The ID of the user updating the label.</param>
        /// <returns>Returns a response containing the updated label entity.</returns>
        Task<ResponseDTO<LabelEntity>> UpdateLabelAsync(LabelUpdateDTO request, int userId);

        /// <summary>
        /// Gets a label by its ID for a specific user.
        /// </summary>
        /// <param name="labelId">The ID of the label to retrieve.</param>
        /// <param name="userId">The ID of the user requesting the label.</param>
        /// <returns>Returns a response containing the label entity.</returns>
        Task<ResponseDTO<LabelEntity>> GetLabelByIdAsync(int labelId, int userId);
    }
}
