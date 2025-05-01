using BusinessLayer.Interface;
using ModelLayer.Entity;
using RepoLayer.DTO;
using RepoLayer.Interface;

namespace BusinessLayer.Service
{
    public class UserImplBL : IUserBL
    {
        public IUserRL _userRL;
        public UserImplBL(IUserRL userRL)
        {
            _userRL = userRL;
        }

        public async Task<ResponseDTO<UserDTO>> AddUser(UserDTO user)
        {
            try
            {
                return await _userRL.AddUser(user);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<UserDTO> { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<ResponseDTO<string>> GetUserByName(string firstName)
        {
            try
            {
                return await _userRL.GetUserByName(firstName);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<string> { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<ResponseDTO<string>> DeleteUser(string email)
        {
            try
            {
                return await _userRL.DeleteUser(email);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<string> { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<ResponseDTO<UserEntity>> Login(LoginDTO loginRequest)
        {
            try
            {
                return await _userRL.Login(loginRequest);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<UserEntity> { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<ResponseDTO<UserEntity>> UpdateUser(string email, string firstName, string lastName)
        {
            try
            {
                return await _userRL.UpdateUser(email, firstName, lastName);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<UserEntity> { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<ResponseDTO<UserEntity>> ChangePassword(string email, string oldPassword, string newPassword)
        {
            try
            {
                return await _userRL.ChangePassword(email, oldPassword, newPassword);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<UserEntity> { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<ResponseDTO<string>> ForgetPassword(string email)
        {
            try
            {
                return await _userRL.ForgetPassword(email);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<string> { IsSuccess = false, Message = ex.Message };
            }
        }

        public async Task<ResponseDTO<string>> SendEmail(string email)
        {
            try
            {
                return await _userRL.SendEmail(email);
            }
            catch (Exception ex)
            {
                return new ResponseDTO<string> { IsSuccess = false, Message = ex.Message };
            }
        }
    }
}
