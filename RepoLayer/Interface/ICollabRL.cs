using ModelLayer.Entity;
using RepoLayer.DTO;

namespace RepoLayer.Interface
{
    public interface ICollabRL
    {
        Task<ResponseDTO<string>> AddCollaboratorAsync(CollabDTO request, int userId);
        Task<ResponseDTO<string>> RemoveCollaboratorAsync(CollabDTO request, int userId);
        Task<ResponseDTO<List<CollabEntity>>> GetAllCollaboratorsAsync(int noteId);
    }
}
