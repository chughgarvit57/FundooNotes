/// <copyright file="LabelController.cs" company="FundooNotes">
/// Copyright (c) FundooNotes. All rights reserved.
/// </copyright>
/// <author>Garvit Chugh</author>
/// <date>Generated on: @DateTime.Now.ToString("yyyy-MM-dd")</date>
/// 
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepoLayer.DTO;
using RepoLayer.Helper;

namespace FundooNotes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// Controller for handling user operations in the Fundoo Notes application.
        /// Provides endpoints for user registration, authentication, profile management, and password operations.
        /// </summary>
        /// <remarks>
        /// This controller includes functionality for:
        /// - User registration and login
        /// - User profile updates (name changes)
        /// - Password management (change, reset)
        /// - Email operations
        /// All sensitive operations require authentication except for registration, login, and password reset.
        /// </remarks>
        public IUserBL _uberBL { get; set; }
        public AuthService _authService { get; set; }
        public PasswordHashService _passwordHash { get; set; }
        private readonly ILogger<UserController> _logger;

        public UserController(IUserBL uberBL, AuthService service, PasswordHashService passwordHash, ILogger<UserController> logger)
        {
            _uberBL = uberBL;
            _authService = service;
            _passwordHash = passwordHash;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="request">User registration details including email, password, first name and last name</param>
        /// <returns>Response with registered user details or error message</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserDTO request)
        {
            try
            {
                var response = await _uberBL.AddUser(request);
                if (response.IsSuccess)
                {
                    _logger.LogInformation("User registered successfully");
                    return Ok(response);
                }
                else
                {
                    _logger.LogError("User registration failed");
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<UserDTO>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Retrieves user details by first name
        /// </summary>
        /// <param name="firstName">First name of the user to search for</param>
        /// <returns>User details if found, otherwise error message</returns>
        [HttpGet]
        public async Task<IActionResult> GetUserByName([FromForm] string firstName)
        {
            try
            {
                var response = await _uberBL.GetUserByName(firstName);
                if (response.IsSuccess)
                {
                    _logger.LogInformation("User retrieved successfully");
                    return Ok(response);
                }
                else
                {
                    _logger.LogError("User not found");
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<UserDTO>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a user account by email (requires authentication)
        /// </summary>
        /// <param name="email">Email of the user to delete</param>
        /// <returns>Success message if deleted, otherwise error message</returns>
        [HttpDelete("delete")]
        [Authorize]
        public async Task<IActionResult> DeleteUser([FromForm] string email)
        {
            try
            {
                var response = await _uberBL.DeleteUser(email);
                if (response.IsSuccess)
                {
                    _logger.LogInformation("User deleted successfully");
                    return Ok(response);
                }
                else
                {
                    _logger.LogError("User not found");
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<UserDTO>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Authenticates a user and generates a JWT token
        /// </summary>
        /// <param name="request">Login credentials (email and password)</param>
        /// <returns>JWT token if authentication succeeds, otherwise error message</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDTO request)
        {
            try
            {
                var response = await _uberBL.Login(request);
                if (!response.IsSuccess)
                {
                    return BadRequest(new ResponseDTO<string>()
                    {
                        IsSuccess = false,
                        Message = "Invalid credentials"
                    });
                }
                var token = _authService.GenerateToken(response.Data);
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(new ResponseDTO<string>()
                    {
                        IsSuccess = false,
                        Message = "Token generation failed"
                    });
                }
                _logger.LogInformation("User logged in successfully");
                return Ok(new ResponseDTO<string>()
                {
                    IsSuccess = true,
                    Message = "Login successful",
                    Data = token
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<UserDTO>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates user's first and last name (requires authentication)
        /// </summary>
        /// <param name="firstName">New first name</param>
        /// <param name="lastName">New last name</param>
        /// <returns>Updated user details or error message</returns>
        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromForm] string firstName, [FromForm] string lastName)
        {
            try
            {
                var email = User.FindFirst("Email")?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new ResponseDTO<UserDTO>()
                    {
                        IsSuccess = false,
                        Message = "Invalid token!"
                    });
                }
                var response = await _uberBL.UpdateUser(email, firstName, lastName);
                if (response.IsSuccess)
                {
                    _logger.LogInformation("User updated successfully");
                    return Ok(response);
                }
                else
                {
                    _logger.LogError("User not found");
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<UserDTO>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Changes user's password (requires authentication)
        /// </summary>
        /// <param name="oldPassword">Current password</param>
        /// <param name="newPassword">New password</param>
        /// <returns>Success message if password changed, otherwise error message</returns>
        [HttpPost("changePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromForm] string oldPassword, [FromForm] string newPassword)
        {
            try
            {
                var email = User.FindFirst("Email")?.Value;
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(new ResponseDTO<UserDTO>()
                    {
                        IsSuccess = false,
                        Message = "Invalid token!"
                    });
                }
                var response = await _uberBL.ChangePassword(email, oldPassword, newPassword);
                if (response.IsSuccess)
                {
                    _logger.LogInformation("Password changed successfully");
                    return Ok(response);
                }
                else
                {
                    _logger.LogError("User not found");
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<UserDTO>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Initiates password reset process for a user
        /// </summary>
        /// <param name="email">Email of the user who forgot password</param>
        /// <returns>Success message if email exists, otherwise error message</returns>
        [HttpPost("forgetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgetPassword([FromForm] string email)
        {
            try
            {
                var response = await _uberBL.ForgetPassword(email);
                if (response.IsSuccess)
                {
                    _logger.LogInformation("Password reset link sent successfully");
                    return Ok(response);
                }
                else
                {
                    _logger.LogError("User not found");
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<UserDTO>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Sends an email to the specified address (for testing purposes)
        /// </summary>
        /// <param name="email">Email address to send to</param>
        /// <returns>Success message if email sent, otherwise error message</returns>
        [HttpGet("sendEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> SendEmail([FromForm] string email)
        {
            try
            {
                var response = await _uberBL.SendEmail(email);
                if (response.IsSuccess)
                {
                    _logger.LogInformation("Email sent successfully");
                    return Ok(response);
                }
                else
                {
                    _logger.LogError("Email not sent");
                    return NotFound(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<string>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }
    }
}