using BusinessLayer.Interface;
using ModelLayer.Entity;
using RepoLayer.DTO;
using RepoLayer.Interface;

namespace BusinessLayer.Service
{
    public class LabelImplBL : ILabelBL
    {
        public ILabelRL _labelRL;
        public LabelImplBL(ILabelRL labelRL)
        {
            _labelRL = labelRL;
        }
        public async Task<ResponseDTO<LabelEntity>> CreateLabelAsync(string labelName, int userId)
        {
            try
            {
                return await _labelRL.CreateLabelAsync(labelName, userId);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseDTO<LabelEntity>> DeleteLabelAsync(string labelName, int userId)
        {
            try
            {
                return await _labelRL.DeleteLabelAsync(labelName, userId);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseDTO<LabelEntity>> AddLabelToNoteAsync(string labelName, int noteId, int userId)
        {
            try
            {
                return await _labelRL.AddLabelToNoteAsync(labelName, noteId, userId);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseDTO<List<LabelEntity>>> ViewAllLabelsAsync(int userId)
        {
            try
            {
                return await _labelRL.ViewAllLabelsAsync(userId);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<List<LabelEntity>>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseDTO<LabelEntity>> UpdateLabelAsync(LabelUpdateDTO request, int userId)
        {
            try
            {
                return await _labelRL.UpdateLabelAsync(request, userId);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResponseDTO<LabelEntity>> GetLabelByIdAsync(int labelId, int userId)
        {
            try
            {
                return await _labelRL.GetLabelByIdAsync(labelId, userId);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}
