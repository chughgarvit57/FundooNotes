using ModelLayer.Entity;
using RepoLayer.DTO;

namespace RepoLayer.Interface
{
    public interface IUserRL
    {
        /// <summary>
        /// Adds a new user to the system.
        /// </summary>
        /// <param name="user">The user details to be added.</param>
        /// <returns>Returns a response with the user DTO after adding the user.</returns>
        Task<ResponseDTO<UserDTO>> AddUser(UserDTO user);

        /// <summary>
        /// Retrieves a user by their first name.
        /// </summary>
        /// <param name="firstName">The first name of the user to be retrieved.</param>
        /// <returns>Returns a response containing a message with the result of the retrieval.</returns>
        Task<ResponseDTO<string>> GetUserByName(string firstName);

        /// <summary>
        /// Deletes a user based on their email.
        /// </summary>
        /// <param name="email">The email address of the user to be deleted.</param>
        /// <returns>Returns a response containing a message indicating the result of the deletion.</returns>
        Task<ResponseDTO<string>> DeleteUser(string email);

        /// <summary>
        /// Authenticates a user by checking their login credentials.
        /// </summary>
        /// <param name="loginRequest">The login request containing the user's credentials.</param>
        /// <returns>Returns a response containing the authenticated user's entity.</returns>
        Task<ResponseDTO<UserEntity>> Login(LoginDTO loginRequest);

        /// <summary>
        /// Updates the user’s details.
        /// </summary>
        /// <param name="email">The email of the user to be updated.</param>
        /// <param name="firstName">The new first name of the user.</param>
        /// <param name="lastName">The new last name of the user.</param>
        /// <returns>Returns a response containing the updated user entity.</returns>
        Task<ResponseDTO<UserEntity>> UpdateUser(string email, string firstName, string lastName);

        /// <summary>
        /// Changes the user's password.
        /// </summary>
        /// <param name="email">The email of the user whose password is to be changed.</param>
        /// <param name="oldPassword">The user's current password.</param>
        /// <param name="newPassword">The new password for the user.</param>
        /// <returns>Returns a response indicating whether the password change was successful.</returns>
        Task<ResponseDTO<UserEntity>> ChangePassword(string email, string oldPassword, string newPassword);

        /// <summary>
        /// Sends a password reset email to the user.
        /// </summary>
        /// <param name="email">The email of the user who requested the password reset.</param>
        /// <returns>Returns a response containing a message indicating the result of the password reset request.</returns>
        Task<ResponseDTO<string>> ForgetPassword(string email);

        /// <summary>
        /// Sends a verification or confirmation email to the user.
        /// </summary>
        /// <param name="email">The email address to which the confirmation message will be sent.</param>
        /// <returns>Returns a response containing a message indicating the result of the email sending operation.</returns>
        Task<ResponseDTO<string>> SendEmail(string email);
    }
}
