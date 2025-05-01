using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using ModelLayer.Entity;
using RepoLayer.DTO;

namespace BusinessLayer.Interface
{
    public interface IUserBL
    {
        /// <summary>
        /// Adds a new user to the system.
        /// </summary>
        /// <param name="user">The user details to add.</param>
        /// <returns>A response containing the added user details.</returns>
        Task<ResponseDTO<UserDTO>> AddUser(UserDTO user);

        /// <summary>
        /// Retrieves a user by their first name.
        /// </summary>
        /// <param name="firstName">The first name of the user to retrieve.</param>
        /// <returns>A response containing the user's information.</returns>
        Task<ResponseDTO<string>> GetUserByName(string firstName);

        /// <summary>
        /// Deletes a user by their email address.
        /// </summary>
        /// <param name="email">The email of the user to delete.</param>
        /// <returns>A response containing a success or failure message.</returns>
        Task<ResponseDTO<string>> DeleteUser(string email);

        /// <summary>
        /// Logs a user into the system.
        /// </summary>
        /// <param name="loginRequest">The login credentials.</param>
        /// <returns>A response containing the logged-in user entity.</returns>
        Task<ResponseDTO<UserEntity>> Login(LoginDTO loginRequest);

        /// <summary>
        /// Updates the first and last name of a user based on their email.
        /// </summary>
        /// <param name="email">The email of the user to update.</param>
        /// <param name="firstName">The new first name.</param>
        /// <param name="lastName">The new last name.</param>
        /// <returns>A response containing the updated user entity.</returns>
        Task<ResponseDTO<UserEntity>> UpdateUser(string email, string firstName, string lastName);

        /// <summary>
        /// Changes the password of a user.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="oldPassword">The current password.</param>
        /// <param name="newPassword">The new password to set.</param>
        /// <returns>A response containing the updated user entity.</returns>
        Task<ResponseDTO<UserEntity>> ChangePassword(string email, string oldPassword, string newPassword);

        /// <summary>
        /// Initiates a forgot password process for the user.
        /// </summary>
        /// <param name="email">The email of the user who forgot their password.</param>
        /// <returns>A response containing a success or failure message.</returns>
        Task<ResponseDTO<string>> ForgetPassword(string email);

        /// <summary>
        /// Sends a test email to the user.
        /// </summary>
        /// <param name="email">The email address to which the email will be sent.</param>
        /// <returns>A response containing a success or failure message.</returns>
        Task<ResponseDTO<string>> SendEmail(string email);
    }
}
