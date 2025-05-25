using ModelLayer.Entity;
using RepoLayer.Context;
using RepoLayer.DTO;
using RepoLayer.Interface;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace RepoLayer.Service
{
    public class NotesImplRL : INotesRL
    {
        private readonly UserContext _userContext;
        private readonly ILogger<NotesImplRL> _logger;
        private readonly IDatabase _redisDatabase;
        private readonly IConnectionMultiplexer _redisConnection;
        public NotesImplRL(UserContext userContext, ILogger<NotesImplRL> logger, IConnectionMultiplexer redis)
        {
            _userContext = userContext;
            _logger = logger;
            _redisConnection = redis;
            _redisDatabase = redis.GetDatabase();
        }
        private string GetUserNotesCacheKey(int userId) => $"user:{userId}:notes";
        private string GetNoteCacheKey(int noteId) => $"note:{noteId}";
        private string GetPinnedNotesCacheKey(int userId) => $"user:{userId}:pinnedNotes";
        private string GetNoteByTitleCacheKey(string title) => $"note:title:{title}";
        private string GetArchivedNotesCacheKey(int userId) => $"user:{userId}:archivedNotes";
        public async Task<ResponseDTO<NotesEntity>> CreateNotesAsync(CreateNotesDTO request, int userId)
        {
            try
            {
                _logger.LogInformation($"Attempting to create a new note for user ID {userId} with title: {request.Title}");

                var user = await _userContext.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found when trying to create a note.");
                    return new ResponseDTO<NotesEntity>
                    {
                        IsSuccess = false,
                        Message = "User not found",
                    };
                }

                var notes = new NotesEntity
                {
                    UserId = userId,
                    Title = request.Title,
                    Description = request.Description,
                    Reminder = request.Reminder,
                    BackgroundColor = request.BackgroundColor,
                    Image = request.Image,
                    Pin = request.Pin,
                    Created = request.Created,
                    Edited = request.Edited,
                    Trash = request.Trash,
                    Archive = request.Archive,
                };

                await _userContext.Notes.AddAsync(notes);
                await _userContext.SaveChangesAsync();

                // Cache the individual note by both ID and title
                var serializedNote = JsonSerializer.Serialize(notes);
                await _redisDatabase.StringSetAsync(GetNoteCacheKey(notes.NoteId), serializedNote, TimeSpan.FromMinutes(30));
                await _redisDatabase.StringSetAsync(GetNoteByTitleCacheKey(notes.Title), serializedNote, TimeSpan.FromMinutes(30));

                // Invalidate the user's notes list cache
                await _redisDatabase.KeyDeleteAsync(GetUserNotesCacheKey(userId));

                _logger.LogInformation($"🎉 A new note was created for user ID {userId} with title: {request.Title}");
                return new ResponseDTO<NotesEntity>
                {
                    IsSuccess = true,
                    Message = "Awesome! Your note has been successfully created!",
                    Data = notes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while creating a note for user ID {userId}");
                return new ResponseDTO<NotesEntity>
                {
                    IsSuccess = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
        }
        public async Task<ResponseDTO<List<NotesEntity>>> GetAllNotesAsync(int userId)
        {
            try
            {
                var cacheKey = GetUserNotesCacheKey(userId);
                var cachedNotes = await _redisDatabase.StringGetAsync(cacheKey);
                if (cachedNotes.HasValue)
                {
                    try
                    {
                        var notesList = JsonSerializer.Deserialize<List<NotesEntity>>(cachedNotes);
                        if (notesList != null && notesList.Any())
                        {
                            return new ResponseDTO<List<NotesEntity>>
                            {
                                IsSuccess = true,
                                Message = "Notes retrieved from cache",
                                Data = notesList
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to deserialize cached notes for user ID {userId}");
                        return new ResponseDTO<List<NotesEntity>>
                        {
                            IsSuccess = false,
                            Message = "Failed to deserialize cached notes"
                        };
                    }
                }
                var userNotes = await _userContext.Notes
                    .Where(n => n.UserId == userId)
                    .ToListAsync();

                var sharedNotes = await _userContext.Collaborator
                    .Where(c => c.CollabEmail == _userContext.Users.FirstOrDefault(u => u.Id == userId).Email)
                    .Join(_userContext.Notes, collaborator => collaborator.NoteId, note => note.NoteId, (collaborator, note) => note)
                    .ToListAsync();

                var allNotes = userNotes.Union(sharedNotes).OrderByDescending(n => n.Created).ToList();
                if (!allNotes.Any())
                {
                    return new ResponseDTO<List<NotesEntity>>
                    {
                        IsSuccess = false,
                        Message = "No notes found"
                    };
                }
                var serialisedNotes = JsonSerializer.Serialize(allNotes);
                await _redisDatabase.StringSetAsync(cacheKey, serialisedNotes, TimeSpan.FromMinutes(30));
                return new ResponseDTO<List<NotesEntity>>
                {
                    IsSuccess = true,
                    Message = "Notes retrieved from database!",
                    Data = allNotes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving notes for user ID {userId}");
                return new ResponseDTO<List<NotesEntity>>
                {
                    IsSuccess = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
        }
        public async Task<ResponseDTO<string>> DeleteNoteAsync(string title)
        {
            try
            {
                var note = await _userContext.Notes.FirstOrDefaultAsync(n => n.Title == title);
                if (note == null)
                {
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "Note not found"
                    };
                }

                _userContext.Notes.Remove(note);
                await _userContext.SaveChangesAsync();

                // Clear all relevant cache entries
                await _redisDatabase.KeyDeleteAsync(GetNoteCacheKey(note.NoteId));
                await _redisDatabase.KeyDeleteAsync(GetNoteByTitleCacheKey(title));
                await _redisDatabase.KeyDeleteAsync(GetUserNotesCacheKey(note.UserId));

                return new ResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Note deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting note with title: {title}");
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
        }
        public async Task<ResponseDTO<NotesEntity>> UpdateNoteAsync(UpdateNotesDTO request, int noteId)
        {
            try
            {
                var note = await _userContext.Notes.FindAsync(noteId);
                if (note == null)
                {
                    return new ResponseDTO<NotesEntity>
                    {
                        IsSuccess = false,
                        Message = "Note not found"
                    };
                }
                else
                {
                    note.Title = request.Title;
                    note.Description = request.Description;
                    note.Reminder = request.Reminder;
                    note.BackgroundColor = request.BackgroundColor;
                    note.Image = request.Image;
                    note.Pin = request.Pin;
                    note.Edited = request.Edited;
                    note.Trash = request.Trash;
                    note.Archive = request.Archive;
                    _userContext.Notes.Update(note);
                    await _userContext.SaveChangesAsync();

                    var serializedNote = JsonSerializer.Serialize(note);
                    await _redisDatabase.StringSetAsync(GetNoteCacheKey(note.NoteId), serializedNote, TimeSpan.FromMinutes(30));
                    await _redisDatabase.StringSetAsync(GetNoteByTitleCacheKey(note.Title), serializedNote, TimeSpan.FromMinutes(30));

                    await _redisDatabase.KeyDeleteAsync(GetUserNotesCacheKey(note.UserId));
                    return new ResponseDTO<NotesEntity>
                    {
                        IsSuccess = true,
                        Message = "Note updated successfully",
                        Data = note
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating note with ID: {noteId}");
                return new ResponseDTO<NotesEntity>
                {
                    IsSuccess = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
        }
        public async Task<ResponseDTO<bool>> PinUnpinNoteAsync(int noteId)
        {
            try
            {
                var note = await _userContext.Notes.FirstOrDefaultAsync(n =>  n.NoteId == noteId);
                if (note == null)
                {
                    return new ResponseDTO<bool>
                    {
                        IsSuccess = false,
                        Message = "Note not found",
                    };
                }

                note.Pin = !note.Pin;
                note.Edited = DateTime.UtcNow;

                _userContext.Notes.Update(note);
                await _userContext.SaveChangesAsync();

                var serializedNote = JsonSerializer.Serialize(note);

                await Task.WhenAll(
                    _redisDatabase.StringSetAsync(GetNoteCacheKey(note.NoteId), serializedNote, TimeSpan.FromMinutes(30)),
                    _redisDatabase.StringSetAsync(GetNoteByTitleCacheKey(note.Title), serializedNote, TimeSpan.FromMinutes(30))
                );

                await _redisDatabase.KeyDeleteAsync(GetUserNotesCacheKey(note.UserId));

                return new ResponseDTO<bool>
                {
                    IsSuccess = true,
                    Message = $"Note {(note.Pin ? "pinned" : "unpinned")} successfully",
                    Data = note.Pin
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while pin/unpin note with id: {noteId}");
                return new ResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
        }
        public async Task<ResponseDTO<bool>> ArchiveNoteAsync(int noteId, int userId)
        {
            try
            {
                var note = await _userContext.Notes
                    .FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);

                if (note == null)
                {
                    return new ResponseDTO<bool>
                    {
                        IsSuccess = false,
                        Message = "Note not found or doesn't belong to user"
                    };
                }

                if (note.Archive)
                {
                    return new ResponseDTO<bool>
                    {
                        IsSuccess = true,
                        Message = "Note is already archived",
                    };
                }

                note.Archive = true;
                note.Pin = false;  // Unpin when archiving
                note.Edited = DateTime.UtcNow;

                _userContext.Notes.Update(note);
                await _userContext.SaveChangesAsync();

                // Update cache
                var serializedNote = JsonSerializer.Serialize(note);
                await Task.WhenAll(
                    _redisDatabase.StringSetAsync(GetNoteCacheKey(note.NoteId), serializedNote, TimeSpan.FromMinutes(30)),
                    _redisDatabase.StringSetAsync(GetNoteByTitleCacheKey(note.Title), serializedNote, TimeSpan.FromMinutes(30))
                );

                await Task.WhenAll(
                    _redisDatabase.KeyDeleteAsync(GetUserNotesCacheKey(userId)),
                    _redisDatabase.KeyDeleteAsync(GetPinnedNotesCacheKey(userId)),
                    _redisDatabase.KeyDeleteAsync(GetArchivedNotesCacheKey(userId))
                );

                return new ResponseDTO<bool>
                {
                    IsSuccess = true,
                    Message = "Note archived successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error archiving note {noteId} for user {userId}");
                return new ResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO<bool>> UnArchiveNoteAsync(int noteId, int userId)
        {
            try
            {
                var note = await _userContext.Notes
                    .FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);

                if (note == null)
                {
                    return new ResponseDTO<bool>
                    {
                        IsSuccess = false,
                        Message = "Note not found or doesn't belong to user"
                    };
                }

                // If not archived, return success
                if (!note.Archive)
                {
                    return new ResponseDTO<bool>
                    {
                        IsSuccess = true,
                        Message = "Note is not archived"
                    };
                }

                note.Archive = false;
                note.Edited = DateTime.UtcNow;

                _userContext.Notes.Update(note);
                await _userContext.SaveChangesAsync();

                // Update cache
                var serializedNote = JsonSerializer.Serialize(note);
                await Task.WhenAll(
                    _redisDatabase.StringSetAsync(GetNoteCacheKey(note.NoteId), serializedNote, TimeSpan.FromMinutes(30)),
                    _redisDatabase.StringSetAsync(GetNoteByTitleCacheKey(note.Title), serializedNote, TimeSpan.FromMinutes(30))
                );

                // Invalidate relevant caches
                await Task.WhenAll(
                    _redisDatabase.KeyDeleteAsync(GetUserNotesCacheKey(userId)),
                    _redisDatabase.KeyDeleteAsync(GetArchivedNotesCacheKey(userId))
                );

                return new ResponseDTO<bool>
                {
                    IsSuccess = true,
                    Message = "Note unarchived successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error unarchiving note {noteId} for user {userId}");
                return new ResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
        }
        public async Task<ResponseDTO<string>> BackgroundColorNoteAsync(int noteId, string backgroundColor)
        {
            try
            {
                var note = await _userContext.Notes.FirstOrDefaultAsync(x => x.NoteId == noteId && x.BackgroundColor == backgroundColor);

                if (note == null)
                {
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "Note not found"
                    };
                }

                if (string.IsNullOrWhiteSpace(backgroundColor))
                {
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "Background color cannot be empty"
                    };
                }

                note.BackgroundColor = backgroundColor;
                note.Edited = DateTime.Now;

                _userContext.Notes.Update(note);
                await _userContext.SaveChangesAsync();

                var serializedNote = JsonSerializer.Serialize(note);
                await Task.WhenAll(
                    _redisDatabase.StringSetAsync(GetNoteCacheKey(note.NoteId), serializedNote, TimeSpan.FromMinutes(30)),
                    _redisDatabase.StringSetAsync(GetNoteByTitleCacheKey(note.Title), serializedNote, TimeSpan.FromMinutes(30))
                );

                await _redisDatabase.KeyDeleteAsync(GetUserNotesCacheKey(note.UserId));

                return new ResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Background color updated successfully",
                    Data = backgroundColor
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing background color for note {noteId}");
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = $"Failed to update background color: {ex.Message}"
                };
            }
        }
        public async Task<ResponseDTO<bool>> TrashNoteAsync(int noteId, int userId)
        {
            try
            {
                // Find note by both title and noteId for validation
                var note = await _userContext.Notes
                    .FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);

                if (note == null)
                {
                    return new ResponseDTO<bool>
                    {
                        IsSuccess = false,
                        Message = "Note not found with the specified ID",
                    };
                }

                // Toggle trash status
                note.Trash = true;
                note.Edited = DateTime.UtcNow;

                // If moving to trash, unpin and unarchive
                if (note.Trash)
                {
                    note.Pin = false;
                    note.Archive = false;
                }

                _userContext.Notes.Update(note);
                await _userContext.SaveChangesAsync();

                var serializedNote = JsonSerializer.Serialize(note);
                await Task.WhenAll(
                    _redisDatabase.StringSetAsync(GetNoteCacheKey(note.NoteId), serializedNote, TimeSpan.FromMinutes(30)),
                    _redisDatabase.StringSetAsync(GetNoteByTitleCacheKey(note.Title), serializedNote, TimeSpan.FromMinutes(30))
                );

                await Task.WhenAll(
                    _redisDatabase.KeyDeleteAsync(GetUserNotesCacheKey(note.UserId)),
                    _redisDatabase.KeyDeleteAsync(GetPinnedNotesCacheKey(note.UserId)),
                    _redisDatabase.KeyDeleteAsync(GetArchivedNotesCacheKey(note.UserId))
                );

                return new ResponseDTO<bool>
                {
                    IsSuccess = true,
                    Message = note.Trash ? "Note moved to trash successfully" : "Note restored from trash successfully",
                    Data = note.Trash
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating trash status for note {noteId}");
                return new ResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO<NotesEntity>> RestoreNoteAsync(int noteId, int userId)
        {
            try
            {
                var note = await _userContext.Notes
                    .FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);

                if (note == null)
                {
                    return new ResponseDTO<NotesEntity>
                    {
                        IsSuccess = false,
                        Message = "Note not found or doesn't belong to user",
                    };
                }

                note.Trash = false;
                _userContext.Notes.Update(note);
                await _userContext.SaveChangesAsync();

                var cacheKey = GetNoteCacheKey(noteId);
                var serializedNote = JsonSerializer.Serialize(note);

                await Task.WhenAll(
                    _redisDatabase.StringSetAsync(cacheKey, serializedNote, TimeSpan.FromMinutes(30)),
                    _redisDatabase.StringSetAsync(GetNoteByTitleCacheKey(note.Title), serializedNote, TimeSpan.FromMinutes(30)),
                    _redisDatabase.KeyDeleteAsync(GetUserNotesCacheKey(userId))
                );

                return new ResponseDTO<NotesEntity>
                {
                    IsSuccess = true,
                    Message = "Note restored successfully",
                    Data = note
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while restoring note with ID: {noteId}");
                return new ResponseDTO<NotesEntity>
                {
                    IsSuccess = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
        }
        public async Task<ResponseDTO<string>> UploadImageAsync(int noteId, int userId, IFormFile imageFile)
        {
            try
            {
                var note = await _userContext.Notes
                    .FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);

                if (note == null)
                {
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "Note not found or doesn't belong to user"
                    };
                }

                if (imageFile == null || imageFile.Length == 0)
                {
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "No image file provided"
                    };
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed."
                    };
                }

                const int maxFileSize = 5 * 1024 * 1024;
                if (imageFile.Length > maxFileSize)
                {
                    return new ResponseDTO<string>
                    {
                        IsSuccess = false,
                        Message = "File size exceeds the 5MB limit"
                    };
                }

                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "NoteImages");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                var relativePath = Path.Combine("NoteImages", fileName);
                note.Image = relativePath;
                note.Edited = DateTime.UtcNow;

                _userContext.Notes.Update(note);
                await _userContext.SaveChangesAsync();

                var serializedNote = JsonSerializer.Serialize(note);
                await Task.WhenAll(
                    _redisDatabase.StringSetAsync(GetNoteCacheKey(note.NoteId), serializedNote, TimeSpan.FromMinutes(30)),
                    _redisDatabase.StringSetAsync(GetNoteByTitleCacheKey(note.Title), serializedNote, TimeSpan.FromMinutes(30))
                );

                await _redisDatabase.KeyDeleteAsync(GetUserNotesCacheKey(userId));

                return new ResponseDTO<string>
                {
                    IsSuccess = true,
                    Message = "Image uploaded successfully",
                    Data = relativePath
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading image for note {noteId} by user {userId}");
                return new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = $"An unexpected error occurred: {ex.Message}"
                };
            }
        }
    }
}