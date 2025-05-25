/// <copyright file="LabelController.cs" company="FundooNotes">
/// Copyright (c) FundooNotes. All rights reserved.
/// </copyright>
/// <author>Garvit Chugh</author>
/// <date>Generated on: @DateTime.Now.ToString("yyyy-MM-dd")</date>

using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Entity;
using RepoLayer.DTO;

namespace FundooNotes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        /// <summary>
        /// Controller for handling notes operations in the Fundoo Notes application.
        /// Provides comprehensive CRUD operations and additional note management features.
        /// </summary>
        /// <remarks>
        /// This controller includes functionality for:
        /// - Creating, reading, updating and deleting notes
        /// - Pinning/unpinning notes
        /// - Archiving/unarchiving notes
        /// - Changing note background colors
        /// - Trash management (moving to trash and restoring)
        /// - Image uploads for notes
        /// All operations require authentication and operate on the authenticated user's notes.
        /// </remarks>
        public INotesBL _notesBL;
        public NotesController(INotesBL notesBL)
        {
            _notesBL = notesBL;
        }
        /// <summary>
        /// Creates a new note for the authenticated user.
        /// </summary>
        /// <param name="request">Contains note details including title, description, color, etc.</param>
        /// <returns>Response with created note details or error message</returns>
        [HttpPost("CreateNotes")]
        [Authorize]
        public async Task<IActionResult> CreateNotes(CreateNotesDTO request)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var result = await _notesBL.CreateNotesAsync(request, userId);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<NotesEntity>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Retrieves all notes belonging to the authenticated user.
        /// </summary>
        /// <returns>List of notes with their details</returns>
        [HttpGet("GetAllNotes")]
        [Authorize]
        public async Task<IActionResult> GetAllNotes()
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var result = await _notesBL.GetAllNotesAsync(userId);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<NotesEntity>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes a note with the specified title.
        /// </summary>
        /// <param name="title">Title of the note to be deleted</param>
        /// <returns>Success/failure status of the deletion operation</returns>
        [HttpDelete("DeleteNote")]
        [Authorize]
        public async Task<IActionResult> DeleteNote([FromForm] string title)
        {
            try
            {
                var result = await _notesBL.DeleteNoteAsync(title);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
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

        /// <summary>
        /// Updates an existing note with new details.
        /// </summary>
        /// <param name="request">Contains updated note information</param>
        /// <returns>Updated note details or error message</returns>
        [HttpPut("UpdateNote")]
        [Authorize]
        public async Task<IActionResult> UpdateNote(UpdateNotesDTO request)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var result = await _notesBL.UpdateNoteAsync(request, userId);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<NotesEntity>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Toggles the pinned status of a note.
        /// </summary>
        /// <param name="title">Title of the note to pin/unpin</param>
        /// <param name="noteId">ID of the note (optional)</param>
        /// <returns>Updated pin status of the note</returns>
        [HttpGet("PinUnpin")]
        [Authorize]
        public async Task<IActionResult> PinUnpinNote([FromForm] int noteId)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var result = await _notesBL.PinUnpinNoteAsync(noteId, userId);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<bool>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Archives a note, moving it to the archived section.
        /// </summary>
        /// <param name="noteId">ID of the note to archive</param>
        /// <returns>Success/failure status of the archive operation</returns>
        [HttpPatch("Archive")]
        [Authorize]
        public async Task<IActionResult> ArchiveNoteAsync([FromForm] int noteId)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var result = await _notesBL.ArchiveNoteAsync(noteId, userId);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<bool>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Unarchives a note, moving it back to the main notes section.
        /// </summary>
        /// <param name="noteId">ID of the note to unarchive</param>
        /// <returns>Success/failure status of the unarchive operation</returns>
        [HttpPatch("UnArchive")]
        [Authorize]
        public async Task<IActionResult> UnArchiveNoteAsync([FromForm] int noteId)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var result = await _notesBL.UnArchiveNoteAsync(noteId, userId);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<bool>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Changes the background color of a note.
        /// </summary>
        /// <param name="backgroundColor">New color to set for the note</param>
        /// <returns>Updated note details with new background color</returns>
        [HttpPatch("BackgroundColorNote")]
        [Authorize]
        public async Task<IActionResult> BackgroundColorNote([FromForm] string backgroundColor, [FromForm] int noteId)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var result = await _notesBL.BackgroundColorNoteAsync(userId, noteId, backgroundColor);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
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

        /// <summary>
        /// Moves a note to trash.
        /// </summary>
        /// <param name="title">Title of the note to move to trash</param>
        /// <returns>Success/failure status of the trash operation</returns>
        [HttpPatch("TrashNote")]
        [Authorize]
        public async Task<IActionResult> TrashNote([FromForm] int noteId)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var result = await _notesBL.TrashNoteAsync(noteId, userId);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<bool>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Restores a note from trash back to active notes.
        /// </summary>
        /// <param name="noteId">ID of the note to restore</param>
        /// <returns>Restored note details or error message</returns>
        [HttpPatch("RestoreNote")]
        [Authorize]
        public async Task<IActionResult> RestoreNote([FromForm] int noteId)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var result = await _notesBL.RestoreNoteAsync(noteId, userId);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<NotesEntity>()
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Uploads an image to associate with a note.
        /// </summary>
        /// <param name="noteId">ID of the note to attach the image to</param>
        /// <param name="imageFile">Image file to upload</param>
        /// <returns>Note details with image URL or error message</returns>
        [HttpPost("UploadImageNote")]
        [Authorize]
        public async Task<IActionResult> UploadImageNote([FromForm] int noteId, IFormFile imageFile)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var result = await _notesBL.UploadImageAsync(noteId, userId, imageFile);
                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
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