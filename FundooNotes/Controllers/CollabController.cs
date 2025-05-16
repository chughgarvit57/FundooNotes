/// <copyright file="CollabController.cs" company="FundooNotes">
/// Copyright (c) FundooNotes. All rights reserved.
/// </copyright>
/// <author>Garvit Chugh</author>
/// <date>Generated on: @DateTime.Now.ToString("yyyy-MM-dd")</date>clear

using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RepoLayer.DTO;

namespace FundooNotes.Controllers
{
    /// <summary>
    /// Controller for handling collaboration operations in the Fundoo Notes application.
    /// Manages sharing notes between users through a collaboration system.
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints for:
    /// - Adding collaborators to notes by email address
    /// - Removing collaborators from notes
    /// All operations require authentication and verify note ownership permissions.
    /// Collaborations enable shared access to notes between multiple users.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class CollabController : ControllerBase
    {
        private readonly ICollabBL _collabBL;
        public CollabController(ICollabBL collabBL)
        {
            _collabBL = collabBL;
        }

        /// <summary>
        /// Adds a collaborator to a note
        /// </summary>
        /// <param name="request">Collaboration details including note ID and collaborator email</param>
        /// <returns>Response with collaboration details or error message</returns>
        /// <response code="200">Returns the newly created collaboration</response>
        /// <response code="400">If the collaboration couldn't be created</response>
        [HttpPost("AddCollaborator")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddCollaborator(CollabDTO request)
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                var result = await _collabBL.AddCollaboratorAsync(request, userId);
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
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                });
            }
        }

        /// <summary>
        /// Removes a collaborator from a note
        /// </summary>
        /// <param name="request">Collaboration details including note ID and collaborator email</param>
        /// <returns>Success message if removed, otherwise error message</returns>
        /// <response code="200">If the collaborator was successfully removed</response>
        /// <response code="400">If the collaborator couldn't be removed</response>
        [HttpDelete("RemoveCollaborator")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemoveCollaborator(CollabDTO request)
        {
            try
            {
                int userId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == "Id")?.Value);
                var result = await _collabBL.RemoveCollaboratorAsync(request, userId);
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
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                });
            }
        }
        /// <summary>
        /// Get All the Collaborator with a specific note id given
        /// </summary>
        [HttpGet("GetAllCollaborators")]
        [Authorize] // requires authentication - requires valid jwt token
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllCollaborators([FromForm] int noteId)
        {
            try
            {
                var result = await _collabBL.GetAllCollaboratorsAsync(noteId);
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
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message,
                });
            }
        }
    }
}