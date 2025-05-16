using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelLayer.Entity;
using RepoLayer.Context;
using RepoLayer.DTO;
using RepoLayer.Interface;
using StackExchange.Redis;

namespace RepoLayer.Service
{
    public class CollabImplRL : ICollabRL
    {
        public UserContext _userContext;
        private readonly ILogger<CollabImplRL> _logger;
        private readonly IDatabase _redisDatabase;
        private readonly IConnectionMultiplexer _redisConnection;
        public CollabImplRL(UserContext userContext, ILogger<CollabImplRL> logger, IConnectionMultiplexer redis)
        {
            _userContext = userContext;
            _logger = logger;
            _redisConnection = redis;
            _redisDatabase = redis.GetDatabase();
        }
        private string GetNoteCollaboratorKey(int noteId) => $"note:{noteId}:collaborators";
        public async Task<ResponseDTO<string>> AddCollaboratorAsync(CollabDTO request, int userId)
        {
            try
            {
                _logger.LogInformation($"Attempting to add collaborator {request.CollabEmail} for NoteId {request.NoteId} by UserId {userId}");

                // Validate note ownership
                var note = await _userContext.Notes.FirstOrDefaultAsync(n => n.NoteId == request.NoteId && n.UserId == userId);
                if (note == null)
                {
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "Note not found or does not belong to the user."
                    };
                }

                // Optionally validate collaborator email exists
                var userExists = await _userContext.Users.AnyAsync(u => u.Email == request.CollabEmail);
                if (!userExists)
                {
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "Collaborator email does not exist in the system."
                    };
                }

                var collaborator = await _userContext.Collaborator.FirstOrDefaultAsync(c => c.CollabEmail == request.CollabEmail && c.UserId == userId && c.NoteId == request.NoteId);
                if (collaborator != null)
                {
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "Collaborator already exists."
                    };
                }
                var newCollaborator = new CollabEntity
                {
                    CollabEmail = request.CollabEmail,
                    UserId = userId,
                    NoteId = request.NoteId,
                    CreatedAt = DateTime.UtcNow
                };
                await _userContext.Collaborator.AddAsync(newCollaborator);
                await _userContext.SaveChangesAsync();

                // Update Redis cache
                var serialisedCollab = JsonSerializer.Serialize(newCollaborator);
                await _redisDatabase.ListRightPushAsync(GetNoteCollaboratorKey(request.NoteId), serialisedCollab);
                await _redisDatabase.KeyExpireAsync(GetNoteCollaboratorKey(request.NoteId), TimeSpan.FromMinutes(30));

                _logger.LogInformation($"Collaborator {request.CollabEmail} added successfully for NoteId {request.NoteId} by UserId {userId}");
                return new ResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Collaborator added successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding collaborator.");
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = $"Failed to add collaborator: {ex.Message}"
                };
            }
        }
        public async Task<ResponseDTO<string>> RemoveCollaboratorAsync(CollabDTO request, int userId)
        {
            try
            {
                _logger.LogInformation($"Attempting to remove collaborator {request.CollabEmail} for NoteId {request.NoteId} by UserId {userId}");

                // Validate note ownership
                var note = await _userContext.Notes.FirstOrDefaultAsync(n => n.NoteId == request.NoteId && n.UserId == userId);
                if (note == null)
                {
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "Note not found or does not belong to the user."
                    };
                }

                var collaborator = await _userContext.Collaborator.FirstOrDefaultAsync(c => c.CollabEmail == request.CollabEmail && c.UserId == userId && c.NoteId == request.NoteId);
                if (collaborator == null)
                {
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "Collaborator does not exist."
                    };
                }
                _userContext.Collaborator.Remove(collaborator);
                await _userContext.SaveChangesAsync();

                // Update Redis cache by rebuilding the list
                var cachedCollaborators = await _redisDatabase.ListRangeAsync(GetNoteCollaboratorKey(request.NoteId));
                var updatedCollaborators = new List<RedisValue>();
                foreach (var item in cachedCollaborators)
                {
                    try
                    {
                        var collab = JsonSerializer.Deserialize<CollabEntity>(item);
                        if (collab != null && collab.CollabEmail != request.CollabEmail)
                        {
                            updatedCollaborators.Add(item);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize collaborator for NoteId {NoteId}", request.NoteId);
                    }
                }

                // Clear and repopulate the Redis list
                await _redisDatabase.KeyDeleteAsync(GetNoteCollaboratorKey(request.NoteId));
                if (updatedCollaborators.Any())
                {
                    await _redisDatabase.ListRightPushAsync(GetNoteCollaboratorKey(request.NoteId), updatedCollaborators.ToArray());
                    await _redisDatabase.KeyExpireAsync(GetNoteCollaboratorKey(request.NoteId), TimeSpan.FromMinutes(30));
                }

                _logger.LogInformation($"Collaborator {request.CollabEmail} removed successfully for NoteId {request.NoteId} by UserId {userId}");
                return new ResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Collaborator removed successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while removing collaborator.");
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = $"Failed to remove collaborator: {ex.Message}"
                };
            }
        }
        public async Task<ResponseDTO<List<CollabEntity>>> GetAllCollaboratorsAsync(int noteId)
        {
            try
            {
                var cachedCollaborators = await _redisDatabase.ListRangeAsync(GetNoteCollaboratorKey(noteId));
                if (cachedCollaborators.Length > 0)
                {
                    var collaborators = new List<CollabEntity>();
                    foreach (var item in cachedCollaborators)
                    {
                        try
                        {
                            var collaborator = JsonSerializer.Deserialize<CollabEntity>(item);
                            if (collaborator != null)
                            {
                                collaborators.Add(collaborator);
                            }
                            else
                            {
                                _logger.LogWarning("Deserialized collaborator is null for NoteId {NoteId}", noteId);
                            }
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogWarning(ex, "Failed to deserialize collaborator for NoteId {NoteId}", noteId);
                        }
                    }
                    if (collaborators.Any())
                    {
                        return new ResponseDTO<List<CollabEntity>>
                        {
                            IsSuccess = true,
                            Message = "Retrieved from cache",
                            Data = collaborators
                        };
                    }
                }
                var collaboratorsFromDb = await _userContext.Collaborator.Where(c => c.NoteId == noteId).ToListAsync();
                if (!collaboratorsFromDb.Any())
                {
                    return new ResponseDTO<List<CollabEntity>>
                    {
                        IsSuccess = false,
                        Message = "No collaborators found."
                    };
                }
                var serialisedCollabs = collaboratorsFromDb.Select(c => JsonSerializer.Serialize(c)).ToArray();
                await _redisDatabase.ListRightPushAsync(GetNoteCollaboratorKey(noteId), serialisedCollabs.Select(c => (RedisValue)c).ToArray());
                await _redisDatabase.KeyExpireAsync(GetNoteCollaboratorKey(noteId), TimeSpan.FromMinutes(30));
                return new ResponseDTO<List<CollabEntity>>
                {
                    IsSuccess = true,
                    Message = "Retrieved from database",
                    Data = collaboratorsFromDb
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving collaborators.");
                return new ResponseDTO<List<CollabEntity>>
                {
                    IsSuccess = false,
                    Message = $"Failed to retrieve collaborators: {ex.Message}"
                };
            }
        }
    }
}