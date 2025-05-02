using ModelLayer.Entity;
using RepoLayer.DTO;

namespace BusinessLayer.Interface
{
    public interface ILabelBL
    {
        Task<ResponseDTO<LabelEntity>> CreateLabelAsync(string labelName, int userId);
        Task<ResponseDTO<LabelEntity>> DeleteLabelAsync(string labelName, int userId);
        Task<ResponseDTO<LabelEntity>> AddLabelToNoteAsync(string labelName, int noteId, int userId);
        Task<ResponseDTO<List<LabelEntity>>> ViewAllLabelsAsync(int userId);
        Task<ResponseDTO<LabelEntity>> UpdateLabelAsync(LabelUpdateDTO request, int userId);
        Task<ResponseDTO<LabelEntity>> GetLabelByIdAsync(int labelId, int userId);
    }
}
