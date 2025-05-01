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
        
        private string GetLabelsCacheKey(int userId) => $"user{userId}:labels";

        public async Task<ResponseDTO<LabelEntity>> CreateLabelAsync(string labelName, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Attepting to create label: {LabelName} for user: {UserId}", labelName, userId);
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
                await _redisDb.StringSetAsync(GetLabelsCacheKey(userId), JsonSerializer.Serialize(newLabel), TimeSpan.FromMinutes(30));
                _logger.LogInformation("Label created successfully: {LabelId}", newLabel.LabelId);
                await transaction.CommitAsync();
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
                await transaction.RollbackAsync();
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                };
            }
        }

        public async Task<ResponseDTO<LabelEntity>> DeleteLabelAsync(string labelName, int userId)
        {
            using var trsansaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var label = await _context.Labels.FirstOrDefaultAsync(x => x.LabelName == labelName && x.UserId == userId);
                if (label == null)
                {
                    return new ResponseDTO<LabelEntity>
                    {
                        IsSuccess = false,
                        Message = "Label not found!",
                        Data = null,
                    };
                }
                _context.Labels.Remove(label);
                await _context.SaveChangesAsync();
                await _redisDb.KeyDeleteAsync(GetLabelsCacheKey(userId));
                _logger.LogInformation("Label with id:{Id} deleted successfully", label.LabelId);
                await trsansaction.CommitAsync();
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
            using var transaction = await _context.Database.BeginTransactionAsync();
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
                var updatedLabel = await _context.Labels.Include(l => l.LabelNotes).FirstOrDefaultAsync(x => x.LabelId == label.LabelId);
                await _redisDb.StringSetAsync(GetLabelsCacheKey(userId), JsonSerializer.Serialize(updatedLabel), TimeSpan.FromMinutes(30));
                await transaction.CommitAsync();
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = true,
                    Message = "Label added to note successfully",
                    Data = label,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding label to note: {Error}", ex.Message);
                await transaction.RollbackAsync();
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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Attempting to update label with name: {LabelName} to {NewLabelName}", request.OldLabelName, request.NewLabelName);
                var label = _context.Labels.FirstOrDefault(lbl => lbl.LabelName == request.OldLabelName && lbl.Users.Id == userId);
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
                var serialisedLabel = JsonSerializer.Serialize(label);
                await _redisDb.StringSetAsync(GetLabelsCacheKey(userId), serialisedLabel, TimeSpan.FromMinutes(30));
                _logger.LogInformation("Label updated successfully: {LabelId}", label.LabelId);
                await transaction.CommitAsync();
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
                await transaction.RollbackAsync();
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
                var caheKey = GetLabelsCacheKey(userId);
                var cachedLabel = await _redisDb.StringGetAsync(caheKey);
                if (cachedLabel.HasValue)
                {
                    var label = JsonSerializer.Deserialize<LabelEntity>(cachedLabel);
                    if (label != null)
                    {
                        return new ResponseDTO<LabelEntity>
                        {
                            IsSuccess = true,
                            Message = "Label Retrieved From Cache",
                            Data = label
                        };
                    }
                }
                var labelFromDb = await _context.Labels.FindAsync(labelId);
                if(labelFromDb == null)
                {
                    return new ResponseDTO<LabelEntity>
                    {
                        IsSuccess = false,
                        Message = "Label Not Found!",
                    };
                }
                await _redisDb.StringSetAsync(caheKey, JsonSerializer.Serialize(labelFromDb), TimeSpan.FromMinutes(30));
                return new ResponseDTO<LabelEntity>
                {
                    IsSuccess = true,
                    Message = "Label retrieved from database",
                    Data = labelFromDb
                };
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
