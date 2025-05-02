using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using ModelLayer.Entity;
using RepoLayer.DTO;

namespace BusinessLayer.Interface
{
    public interface IUserBL
    {
        Task<ResponseDTO<UserDTO>> AddUser(UserDTO user);
        Task<ResponseDTO<string>> GetUserByName(string firstName);
        Task<ResponseDTO<string>> DeleteUser(string email);
        Task<ResponseDTO<UserEntity>> Login(LoginDTO loginRequest);
        Task<ResponseDTO<UserEntity>> UpdateUser(string email, string firstName, string lastName);
        Task<ResponseDTO<UserEntity>> ChangePassword(string email, string oldPassword, string newPassword);
        Task<ResponseDTO<string>> ForgetPassword(string email);
        Task<ResponseDTO<string>> SendEmail(string email);
    }
}
