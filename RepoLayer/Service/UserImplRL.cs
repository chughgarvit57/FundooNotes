using ConsumerLayer.Consumer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelLayer.Entity;
using RepoLayer.Context;
using RepoLayer.DTO;
using RepoLayer.Helper;
using RepoLayer.Interface;
using StackExchange.Redis;
using System.Text.Json;

namespace RepoLayer.Service
{
    public class UserImplRL : IUserRL
    {
        public UserContext _context { get; set; }
        private readonly PasswordHashService _passwordHash;
        public RabbitMQProducer _rabbitMQProducer;
        public RabbitMQConsumer _rabbitMQConsumer;
        public EmailService _emailService;
        private readonly ILogger<UserImplRL> _logger;
        private readonly StackExchange.Redis.IDatabase _redisDatabase;
        private readonly IConnectionMultiplexer _redisConnection;

        public UserImplRL(UserContext context, PasswordHashService passHash, RabbitMQProducer rabbitMQProducer,
                         RabbitMQConsumer consumer, EmailService emailService, ILogger<UserImplRL> logger,
                         IConnectionMultiplexer redis)
        {
            _context = context;
            _passwordHash = passHash;
            _rabbitMQProducer = rabbitMQProducer;
            _rabbitMQConsumer = consumer;
            _emailService = emailService;
            _logger = logger;
            _redisConnection = redis;
            _redisDatabase = redis.GetDatabase();
        }

        private string GetUserCacheKey(string email) => $"user:{email}";
        private string GetUserByNameCacheKey(string firstName) => $"userByName:{firstName}";

        public async Task<ResponseDTO<UserDTO>> AddUser(UserDTO request)
        {
            try
            {
                _logger.LogInformation("Attempting to register user with email: {Email}", request.Email);

                // Check cache first
                var cachedUser = await _redisDatabase.StringGetAsync(GetUserCacheKey(request.Email));
                if (cachedUser.HasValue)
                {
                    _logger.LogWarning("User already exists in cache with email: {Email}", request.Email);
                    return new ResponseDTO<UserDTO>
                    {
                        IsSuccess = false,
                        Message = "User already exists with this email!"
                    };
                }

                var user = await _context.Users.SingleOrDefaultAsync(x => x.Email == request.Email);
                if (user != null)
                {
                    _logger.LogWarning("User already exists in database with email: {Email}", request.Email);
                    return new ResponseDTO<UserDTO>
                    {
                        IsSuccess = false,
                        Message = "User already exists with this email!"
                    };
                }

                string hashedPassword = _passwordHash.HashPassword(request.Password);
                var userEntity = new UserEntity
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Password = hashedPassword,
                };

                await _context.Users.AddAsync(userEntity);
                await _context.SaveChangesAsync();

                // Add to Redis cache
                var cacheKey = GetUserCacheKey(request.Email);
                var serializedUser = JsonSerializer.Serialize(userEntity);
                await _redisDatabase.StringSetAsync(cacheKey, serializedUser, TimeSpan.FromMinutes(30));

                // Also cache by name
                var nameCacheKey = GetUserByNameCacheKey(request.FirstName);
                await _redisDatabase.StringSetAsync(nameCacheKey, serializedUser, TimeSpan.FromMinutes(30));

                var message = new EmailMessageDTO
                {
                    To = request.Email,
                    Subject = "🎉 Welcome to Our Platform!",
                    Body = $"Hey {request.FirstName} {request.LastName}! 👋<br/><br/>" +
                           "We're super excited to have you on board! 🚀<br/>" +
                           "Get ready to explore awesome features and make your journey with us amazing. 💫<br/><br/>" +
                           "Cheers,<br/>The Fundoo Team ❤️"
                };
                _emailService.SendEmail(message.To, message.Subject, message.Body);

                _logger.LogInformation("User registered and welcome email sent to: {Email}", request.Email);
                request.Password = hashedPassword;

                return new ResponseDTO<UserDTO>
                {
                    IsSuccess = true,
                    Message = "User registered successfully",
                    Data = request
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering user: {Email}", request.Email);
                return new ResponseDTO<UserDTO>
                {
                    IsSuccess = false,
                    Message = $"An error occurred while registering the user: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO<string>> GetUserByName(string firstName)
        {
            try
            {
                _logger.LogInformation("Checking Redis cache for user: {firstName}", firstName);
                var nameCacheKey = GetUserByNameCacheKey(firstName);
                var cachedUser = await _redisDatabase.StringGetAsync(nameCacheKey);

                if (cachedUser.HasValue)
                {
                    var userEntity = JsonSerializer.Deserialize<UserEntity>(cachedUser);
                    _logger.LogInformation("User found in cache: {firstName}", firstName);
                    return new ResponseDTO<string>
                    {
                        IsSuccess = true,
                        Message = "User found in cache",
                        Data = $"{userEntity.FirstName} {userEntity.LastName}, Email: {userEntity.Email}"
                    };
                }

                _logger.LogInformation("User not found in cache, querying database for: {firstName}", firstName);
                var user = await _context.Users.SingleOrDefaultAsync(x => x.FirstName == firstName);
                if (user == null)
                {
                    _logger.LogWarning("User not found with first name: {FirstName}", firstName);
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };
                }

                // Cache the user data
                var serializedUser = JsonSerializer.Serialize(user);
                await _redisDatabase.StringSetAsync(nameCacheKey, serializedUser, TimeSpan.FromMinutes(30));
                await _redisDatabase.StringSetAsync(GetUserCacheKey(user.Email), serializedUser, TimeSpan.FromMinutes(30));

                var userInfo = $"{user.FirstName} {user.LastName}, Email: {user.Email}";

                return new ResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "User found",
                    Data = userInfo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving user by first name: {FirstName}", firstName);
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = $"An error occurred while retrieving the user: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO<string>> DeleteUser(string email)
        {
            try
            {
                _logger.LogInformation("Deleting user with email: {Email}", email);

                var cacheKey = GetUserCacheKey(email);
                var cachedUser = await _redisDatabase.StringGetAsync(cacheKey);

                // Always fetch from DB to ensure entity tracking is correct
                var userFound = await _context.Users.SingleOrDefaultAsync(x => x.Email == email);
                if (userFound == null)
                {
                    _logger.LogWarning("User not found for deletion: {Email}", email);
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };
                }

                // Remove user from DB
                _context.Users.Remove(userFound);
                await _context.SaveChangesAsync();

                // Remove from cache (use DB version to access properties safely)
                await _redisDatabase.KeyDeleteAsync(cacheKey);
                await _redisDatabase.KeyDeleteAsync(GetUserByNameCacheKey(userFound.FirstName));

                _logger.LogInformation("User deleted successfully: {Email}", email);
                return new ResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "User deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user: {Email}", email);
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = $"An error occurred while deleting the user: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO<UserEntity>> Login(LoginDTO loginRequest)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", loginRequest.Email);

                // Check cache first
                var cacheKey = GetUserCacheKey(loginRequest.Email);
                var cachedUser = await _redisDatabase.StringGetAsync(cacheKey);

                // Handle cached user case
                if (cachedUser.HasValue)
                {
                    var cachedUserEntity = JsonSerializer.Deserialize<UserEntity>(cachedUser);
                    _logger.LogInformation("User found in cache for login: {Email}", loginRequest.Email);

                    if (_passwordHash.VerifyPassword(loginRequest.Password, cachedUserEntity.Password))
                    {
                        _logger.LogInformation("Login successful for email: {Email}", loginRequest.Email);
                        return new ResponseDTO<UserEntity>
                        {
                            IsSuccess = true,
                            Message = "Login successful",
                            Data = cachedUserEntity
                        };
                    }

                    _logger.LogWarning("Login failed: invalid password for email: {Email}", loginRequest.Email);
                    return new ResponseDTO<UserEntity>
                    {
                        IsSuccess = false,
                        Message = "Invalid password!"
                    };
                }

                // Handle database case
                var dbUser = await _context.Users.SingleOrDefaultAsync(x => x.Email == loginRequest.Email);
                if (dbUser == null)
                {
                    _logger.LogWarning("Login failed: user not found for email: {Email}", loginRequest.Email);
                    return new ResponseDTO<UserEntity>
                    {
                        IsSuccess = false,
                        Message = "User not found!"
                    };
                }

                // Cache the user data since it wasn't in cache
                var serializedUser = JsonSerializer.Serialize(dbUser);
                await _redisDatabase.StringSetAsync(cacheKey, serializedUser, TimeSpan.FromMinutes(30));

                if (_passwordHash.VerifyPassword(loginRequest.Password, dbUser.Password))
                {
                    _logger.LogInformation("Login successful for email: {Email}", loginRequest.Email);
                    return new ResponseDTO<UserEntity>
                    {
                        IsSuccess = true,
                        Message = "Login successful",
                        Data = dbUser
                    };
                }

                _logger.LogWarning("Login failed: invalid password for email: {Email}", loginRequest.Email);
                return new ResponseDTO<UserEntity>
                {
                    IsSuccess = false,
                    Message = "Invalid password!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for email: {Email}", loginRequest.Email);
                return new ResponseDTO<UserEntity>
                {
                    IsSuccess = false,
                    Message = $"An error occurred while logging in: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO<UserEntity>> UpdateUser(string email, string firstName, string lastName)
        {
            try
            {
                _logger.LogInformation("Updating user with email: {Email}", email);

                var cacheKey = GetUserCacheKey(email);
                var cachedUser = await _redisDatabase.StringGetAsync(cacheKey);

                // Handle cached user case
                if (cachedUser.HasValue)
                {
                    var cachedUserEntity = JsonSerializer.Deserialize<UserEntity>(cachedUser);
                    await _redisDatabase.KeyDeleteAsync(cacheKey);
                    await _redisDatabase.KeyDeleteAsync(GetUserByNameCacheKey(cachedUserEntity.FirstName));

                    return await UpdateAndCacheUser(cachedUserEntity, firstName, lastName, cacheKey);
                }

                // Handle database case
                var dbUser = await _context.Users.SingleOrDefaultAsync(x => x.Email == email);
                if (dbUser == null)
                {
                    _logger.LogWarning("Update failed: user not found with email: {Email}", email);
                    return new ResponseDTO<UserEntity>
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };
                }

                return await UpdateAndCacheUser(dbUser, firstName, lastName, cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating user: {Email}", email);
                return new ResponseDTO<UserEntity>
                {
                    IsSuccess = false,
                    Message = $"An error occurred while updating the user: {ex.Message}"
                };
            }
        }

        private async Task<ResponseDTO<UserEntity>> UpdateAndCacheUser(UserEntity user, string firstName, string lastName, string cacheKey)
        {
            try
            {
                var oldFirstName = user.FirstName;
                user.FirstName = firstName;
                user.LastName = lastName;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Update cache
                var serializedUser = JsonSerializer.Serialize(user);
                await _redisDatabase.StringSetAsync(cacheKey, serializedUser, TimeSpan.FromMinutes(30));
                await _redisDatabase.StringSetAsync(GetUserByNameCacheKey(firstName), serializedUser, TimeSpan.FromMinutes(30));

                // Remove old name cache if changed
                if (oldFirstName != firstName)
                {
                    await _redisDatabase.KeyDeleteAsync(GetUserByNameCacheKey(oldFirstName));
                }

                _logger.LogInformation("User updated successfully: {Email}", user.Email);
                return new ResponseDTO<UserEntity>
                {
                    IsSuccess = true,
                    Message = "User updated successfully",
                    Data = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {Email} in UpdateAndCacheUser", user.Email);
                await _redisDatabase.KeyDeleteAsync(cacheKey);
                await _redisDatabase.KeyDeleteAsync(GetUserByNameCacheKey(firstName));
                await _redisDatabase.KeyDeleteAsync(GetUserByNameCacheKey(user.FirstName));

                return new ResponseDTO<UserEntity>
                {
                    IsSuccess = false,
                    Message = $"Failed to update user: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO<UserEntity>> ChangePassword(string email, string oldPassword, string newPassword)
        {
            try
            {
                _logger.LogInformation("Changing password for email: {Email}", email);

                var cacheKey = GetUserCacheKey(email);
                var cachedUser = await _redisDatabase.StringGetAsync(cacheKey);

                if (cachedUser.HasValue)
                {
                    try
                    {
                        var cachedUserEntity = JsonSerializer.Deserialize<UserEntity>(cachedUser);
                        return await ProcessPasswordChange(cachedUserEntity, oldPassword, newPassword, cacheKey);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing cached user for email: {Email}", email);
                        return new ResponseDTO<UserEntity>
                        {
                            IsSuccess = false,
                            Message = "Error processing cached user"
                        };
                    }
                }

                try
                {
                    var dbUser = await _context.Users.SingleOrDefaultAsync(x => x.Email == email);
                    if (dbUser == null)
                    {
                        _logger.LogWarning("Password change failed: user not found for email: {Email}", email);
                        return new ResponseDTO<UserEntity>
                        {
                            IsSuccess = false,
                            Message = "User not found"
                        };
                    }

                    return await ProcessPasswordChange(dbUser, oldPassword, newPassword, cacheKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving user from database for email: {Email}", email);
                    return new ResponseDTO<UserEntity>
                    {
                        IsSuccess = false,
                        Message = "Error retrieving user from database"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing password for email: {Email}", email);
                return new ResponseDTO<UserEntity>
                {
                    IsSuccess = false,
                    Message = $"An error occurred while changing the password: {ex.Message}"
                };
            }
        }

        private async Task<ResponseDTO<UserEntity>> ProcessPasswordChange(
            UserEntity user,
            string oldPassword,
            string newPassword,
            string cacheKey)
        {
            try
            {
                if (!_passwordHash.VerifyPassword(oldPassword, user.Password))
                {
                    _logger.LogWarning("Password change failed: incorrect old password for email: {Email}", user.Email);
                    return new ResponseDTO<UserEntity>
                    {
                        IsSuccess = false,
                        Message = "Old password is incorrect"
                    };
                }

                user.Password = _passwordHash.HashPassword(newPassword);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                try
                {
                    var serializedUser = JsonSerializer.Serialize(user);
                    await _redisDatabase.StringSetAsync(cacheKey, serializedUser, TimeSpan.FromMinutes(30));
                    await _redisDatabase.StringSetAsync(GetUserByNameCacheKey(user.FirstName), serializedUser, TimeSpan.FromMinutes(30));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating cache for user: {Email}", user.Email);
                    return new ResponseDTO<UserEntity>
                    {
                        IsSuccess = false,
                        Message = "Failed to update cache"
                    };
                }

                _logger.LogInformation("Password changed successfully for email: {Email}", user.Email);

                return new ResponseDTO<UserEntity>
                {
                    IsSuccess = true,
                    Message = "Password changed successfully!",
                    Data = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessPasswordChange for email: {Email}", user.Email);
                return new ResponseDTO<UserEntity>
                {
                    IsSuccess = false,
                    Message = $"An error occurred while processing the password change: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO<string>> ForgetPassword(string email)
        {
            try
            {
                _logger.LogInformation("Forgot password request for email: {Email}", email);

                // Check cache first
                var cacheKey = GetUserCacheKey(email);
                var cachedUser = await _redisDatabase.StringGetAsync(cacheKey);

                if (cachedUser.HasValue)
                {
                    var user = JsonSerializer.Deserialize<UserEntity>(cachedUser);
                    _logger.LogInformation("User found in cache for password reset: {Email}", email);

                    var message = new EmailMessageDTO
                    {
                        To = email,
                        Subject = "Reset Password",
                        Body = $"Click here to reset your password: <a href='http://localhost:5000/resetpassword?email={email}'>Reset Password</a>"
                    };

                    _rabbitMQProducer.PublishMessageAsync(message);
                    _rabbitMQConsumer.ConsumeMessagesAsync();

                    _logger.LogInformation("Reset password email queued successfully for: {Email}", email);
                    return new ResponseDTO<string>
                    {
                        IsSuccess = true,
                        Message = "Reset password link sent to your email!",
                    };
                }

                // If not in cache, check database
                var dbUser = await _context.Users.SingleOrDefaultAsync(x => x.Email == email);
                if (dbUser == null)
                {
                    _logger.LogWarning("Forgot password failed: user not found for email: {Email}", email);
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };
                }

                var dbMessage = new EmailMessageDTO
                {
                    To = email,
                    Subject = "Reset Password",
                    Body = $"Click here to reset your password: <a href='http://localhost:5000/resetpassword?email={email}'>Reset Password</a>"
                };

                _rabbitMQProducer.PublishMessageAsync(dbMessage);
                _rabbitMQConsumer.ConsumeMessagesAsync();

                _logger.LogInformation("Reset password email queued successfully for: {Email}", email);
                return new ResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Reset password link sent to your email!",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during forgot password for email: {Email}", email);
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = $"An error occurred while sending the reset password link: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO<string>> SendEmail(string email)
        {
            try
            {
                _logger.LogInformation("Sending email to: {Email}", email);

                // Check cache first
                var cacheKey = GetUserCacheKey(email);
                var cachedUser = await _redisDatabase.StringGetAsync(cacheKey);

                if (cachedUser.HasValue)
                {
                    var cachedUserEntity = JsonSerializer.Deserialize<UserEntity>(cachedUser);
                    _logger.LogInformation("User found in cache for email sending: {Email}", email);

                    var message = new EmailMessageDTO
                    {
                        To = email,
                        Subject = "Welcome to our service",
                        Body = $"Hello {cachedUserEntity.FirstName}, welcome to our service!"
                    };

                    _emailService.SendEmail(message.To, message.Subject, message.Body);

                    _logger.LogInformation("Email sent successfully to: {Email}", email);
                    return new ResponseDTO<string>
                    {
                        IsSuccess = true,
                        Message = "Email sent successfully!",
                    };
                }

                // If not in cache, check database
                var dbUser = await _context.Users.SingleOrDefaultAsync(x => x.Email == email);
                if (dbUser == null)
                {
                    _logger.LogWarning("SendEmail failed: user not found for email: {Email}", email);
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "User not found"
                    };
                }

                var dbMessage = new EmailMessageDTO
                {
                    To = email,
                    Subject = "Welcome to our service",
                    Body = $"Hello {dbUser.FirstName}, welcome to our service!"
                };

                _emailService.SendEmail(dbMessage.To, dbMessage.Subject, dbMessage.Body);

                _logger.LogInformation("Email sent successfully to: {Email}", email);
                return new ResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Email sent successfully!",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email to: {Email}", email);
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = $"An error occurred while sending the email: {ex.Message}"
                };
            }
        }
    }
}