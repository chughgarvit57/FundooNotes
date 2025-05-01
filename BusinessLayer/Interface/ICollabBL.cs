using ModelLayer.Entity;
using RepoLayer.DTO;

namespace BusinessLayer.Interface
{
    public interface ICollabBL
    {
        Task<ResponseDTO<string>> AddCollaboratorAsync(CollabDTO request, int userId);
        Task<ResponseDTO<string>> RemoveCollaboratorAsync(CollabDTO request, int userId);
        Task<ResponseDTO<List<CollabEntity>>> GetAllCollaboratorsAsync(int noteId);
    }
}
