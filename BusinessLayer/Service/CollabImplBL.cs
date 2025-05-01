using BusinessLayer.Interface;
using ModelLayer.Entity;
using RepoLayer.DTO;
using RepoLayer.Interface;

namespace BusinessLayer.Service
{
    public class CollabImplBL : ICollabBL
    {
        public ICollabRL _collabRL;
        public CollabImplBL(ICollabRL collabRL)
        {
            _collabRL = collabRL;
        }
        public async Task<ResponseDTO<string>> AddCollaboratorAsync(CollabDTO request, int userId)
        {
            try
            {
                return await _collabRL.AddCollaboratorAsync(request, userId);
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
        public async Task<ResponseDTO<string>> RemoveCollaboratorAsync(CollabDTO request, int userId)
        {
            try
            {
                return await _collabRL.RemoveCollaboratorAsync(request, userId);
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
        public async Task<ResponseDTO<List<CollabEntity>>> GetAllCollaboratorsAsync(int noteId)
        {
            try
            {
                return await _collabRL.GetAllCollaboratorsAsync(noteId);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<List<CollabEntity>>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}
