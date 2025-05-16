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
    public class LabelImplRL : ILabelRL
    {
        public UserContext _context;
        private readonly IDatabase _redisDb;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly ILogger<LabelImplRL> _logger;
        public LabelImplRL(UserContext context, IConnectionMultiplexer redis, ILogger<LabelImplRL> logger)
        {
            _context = context;
            _redisConnection = redis;
            _redisDb = redis.GetDatabase();
            _logger = logger;
        }

        private string GetLabelsCacheKey(int userId) => $"user:{userId}:labels";
        private string GetLabelByIdCacheKey(int labelId, int userId) => $"user:{userId}:label:{labelId}";

        public async Task<ResponseDTO<LabelEntity>> CreateLabelAsync(string labelName, int userId)
        {
            try
            {
                _logger.LogInformation("Attempting to create label: {LabelName} for user: {UserId}", labelName, userId);
                var label = await _context.Labels.FirstOrDefaultAsync(x => x.LabelName == labelName && x.UserId == userId);
                if (label != null)
                {
                    return new ResponseDTO<LabelEntity>
                    {
                        IsSuccess = false,
                        Message = "Label already exists!",
                    };
                }
                var newLabel = new LabelEntity
                {
                    LabelName = labelName,
                    UserId = userId,
                };
                await _context.Labels.AddAsync(newLabel);
                await _context.SaveChangesAsync();

                // Update the labels list cache
                var labels = await _context.Labels
                    .Where(x => x.UserId == userId)
                    .ToListAsync();
                await _redisDb.StringSetAsync(GetLabelsCacheKey(userId), JsonSerializer.Serialize(labels), TimeSpan.FromMinutes(30));
                await _redisDb.StringSetAsync(GetLabelByIdCacheKey(newLabel.LabelId, userId), JsonSerializer.Serialize(newLabel), TimeSpan.FromMinutes(30));

                _logger.LogInformation("Label created successfully: {LabelId}", newLabel.LabelId);
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = true,
                    Message = "Label created successfully",
                    Data = newLabel,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating label: {Error}", ex.Message);
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<LabelEntity>> DeleteLabelAsync(string labelName, int userId)
        {
            try
            {
                var label = await _context.Labels.FirstOrDefaultAsync(x => x.LabelName == labelName && x.UserId == userId);
                if (label == null)
                {
                    return new ResponseDTO<LabelEntity>
                    {
                        IsSuccess = false,
                        Message = "Label not found!"
                    };
                }
                _context.Labels.Remove(label);
                await _context.SaveChangesAsync();

                // Update the labels list cache
                var labels = await _context.Labels
                    .Where(x => x.UserId == userId)
                    .ToListAsync();
                await _redisDb.StringSetAsync(GetLabelsCacheKey(userId), JsonSerializer.Serialize(labels), TimeSpan.FromMinutes(30));
                await _redisDb.KeyDeleteAsync(GetLabelByIdCacheKey(label.LabelId, userId));

                _logger.LogInformation("Label with id:{Id} deleted successfully", label.LabelId);
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = true,
                    Message = "Label deleted successfully",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting label: {Error}", ex.Message);
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<LabelEntity>> AddLabelToNoteAsync(string labelName, int noteId, int userId)
        {
            try
            {
                var label = await _context.Labels.Include(l => l.LabelNotes).FirstOrDefaultAsync(x => x.LabelName == labelName && x.UserId == userId);
                if (label == null)
                {
                    return new ResponseDTO<LabelEntity>
                    {
                        IsSuccess = false,
                        Message = "Label not found!",
                    };
                }
                var note = await _context.Notes.FirstOrDefaultAsync(x => x.NoteId == noteId && x.UserId == userId);
                if (note == null)
                {
                    return new ResponseDTO<LabelEntity>
                    {
                        IsSuccess = false,
                        Message = "Note not found!",
                    };
                }
                if (label.LabelNotes.Any(n => n.NoteId == noteId))
                {
                    return new ResponseDTO<LabelEntity>
                    {
                        IsSuccess = false,
                        Message = "Label already exists for this note!",
                    };
                }
                var labelNote = new LabelNoteEntity
                {
                    LabelId = label.LabelId,
                    NoteId = note.NoteId,
                };
                note.LabelNotes.Add(labelNote);
                await _context.SaveChangesAsync();

                // Update caches
                var updatedLabel = await _context.Labels.Include(l => l.LabelNotes).FirstOrDefaultAsync(x => x.LabelId == label.LabelId);
                var labels = await _context.Labels
                    .Where(x => x.UserId == userId)
                    .ToListAsync();
                await _redisDb.StringSetAsync(GetLabelsCacheKey(userId), JsonSerializer.Serialize(labels), TimeSpan.FromMinutes(30));
                await _redisDb.StringSetAsync(GetLabelByIdCacheKey(label.LabelId, userId), JsonSerializer.Serialize(updatedLabel), TimeSpan.FromMinutes(30));

                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = true,
                    Message = "Label added to note successfully",
                    Data = updatedLabel,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding label to note: {Error}", ex.Message);
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<List<LabelEntity>>> ViewAllLabelsAsync(int userId)
        {
            try
            {
                var cacheKey = GetLabelsCacheKey(userId);
                var cachedLabels = await _redisDb.StringGetAsync(cacheKey);

                if (cachedLabels.HasValue && !string.IsNullOrWhiteSpace(cachedLabels))
                {
                    try
                    {
                        var labels = JsonSerializer.Deserialize<List<LabelEntity>>(cachedLabels);
                        if (labels != null)
                        {
                            return new ResponseDTO<List<LabelEntity>>
                            {
                                IsSuccess = true,
                                Message = "Labels retrieved from cache",
                                Data = labels,
                            };
                        }
                        _logger.LogWarning("Deserialized labels list is null for user: {UserId}", userId);
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError("Deserialization error: {Error}", jsonEx.Message);
                        await _redisDb.KeyDeleteAsync(cacheKey);
                    }
                }

                var labelsFromDb = await _context.Labels
                    .Where(x => x.UserId == userId)
                    .Select(x => new LabelEntity
                    {
                        LabelId = x.LabelId,
                        LabelName = x.LabelName,
                        UserId = x.UserId,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                    })
                    .ToListAsync();

                if (labelsFromDb.Count == 0)
                {
                    return new ResponseDTO<List<LabelEntity>>
                    {
                        IsSuccess = false,
                        Message = "No labels found!",
                    };
                }

                await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(labelsFromDb), TimeSpan.FromMinutes(30));

                return new ResponseDTO<List<LabelEntity>>
                {
                    IsSuccess = true,
                    Message = "Labels retrieved successfully",
                    Data = labelsFromDb,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving labels: {Error}", ex.Message);
                return new ResponseDTO<List<LabelEntity>>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<LabelEntity>> UpdateLabelAsync(LabelUpdateDTO request, int userId)
        {
            try
            {
                _logger.LogInformation("Attempting to update label with name: {LabelName} to {NewLabelName}", request.OldLabelName, request.NewLabelName);
                var label = await _context.Labels.FirstOrDefaultAsync(lbl => lbl.LabelName == request.OldLabelName && lbl.UserId == userId);
                if (label == null)
                {
                    return new ResponseDTO<LabelEntity>
                    {
                        IsSuccess = false,
                        Message = "Label not found!"
                    };
                }
                label.LabelName = request.NewLabelName;
                label.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Update caches
                var labels = await _context.Labels
                    .Where(x => x.UserId == userId)
                    .ToListAsync();
                await _redisDb.StringSetAsync(GetLabelsCacheKey(userId), JsonSerializer.Serialize(labels), TimeSpan.FromMinutes(30));
                await _redisDb.StringSetAsync(GetLabelByIdCacheKey(label.LabelId, userId), JsonSerializer.Serialize(label), TimeSpan.FromMinutes(30));

                _logger.LogInformation("Label updated successfully: {LabelId}", label.LabelId);
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = true,
                    Message = "Label updated successfully",
                    Data = label,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating label '{request.OldLabelName}'", ex.Message);
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<LabelEntity>> GetLabelByIdAsync(int labelId, int userId)
        {
            try
            {
                var cacheKey = GetLabelByIdCacheKey(labelId, userId);
                var cachedLabel = await _redisDb.StringGetAsync(cacheKey);
                if (cachedLabel.HasValue)
                {
                    var label = JsonSerializer.Deserialize<LabelEntity>(cachedLabel);
                    if (label != null)
                    {
                        return new ResponseDTO<LabelEntity>
                        {
                            IsSuccess = true,
                            Message = "Label retrieved from cache",
                            Data = label
                        };
                    }
                }
                var labelFromDb = await _context.Labels
                    .Where(x => x.LabelId == labelId && x.UserId == userId)
                    .FirstOrDefaultAsync();
                if (labelFromDb == null)
                {
                    return new ResponseDTO<LabelEntity>
                    {
                        IsSuccess = false,
                        Message = "Label not found!",
                    };
                }
                await _redisDb.StringSetAsync(cacheKey, JsonSerializer.Serialize(labelFromDb), TimeSpan.FromMinutes(30));
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = true,
                    Message = "Label retrieved from database",
                    Data = labelFromDb
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving label by ID: {Error}", ex.Message);
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}