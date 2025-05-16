/// <copyright file="LabelController.cs" company="FundooNotes">
/// Copyright (c) FundooNotes. All rights reserved.
/// </copyright>
/// <author>Garvit Chugh</author>
/// <date>Generated on: @DateTime.Now.ToString("yyyy-MM-dd")</date>

using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Entity;
using RepoLayer.DTO;

namespace FundooNotes.Controllers
{
    /// <summary>
    /// Controller for managing note labels and label-note associations
    /// </summary>
    /// <remarks>
    /// This controller handles all operations related to:
    /// - Creating and managing labels
    /// - Associating labels with notes
    /// - Retrieving label information
    /// All endpoints require JWT authentication.
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class LabelController : ControllerBase
    {
        private readonly ILabelBL _userBL;

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelController"/> class.
        /// </summary>
        /// <param name="userBL">Business layer service for label operations</param>
        public LabelController(ILabelBL userBL)
        {
            _userBL = userBL;
        }

        /// <summary>
        /// Creates a new label for the authenticated user
        /// </summary>
        /// <param name="labelName">Name of the label to create</param>
        /// <returns>Response with created label details or error message</returns>
        /// <response code="200">Returns the newly created label</response>
        /// <response code="400">If the label couldn't be created</response>
        [HttpPost("CreateLabel")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateLabel([FromForm] string labelName)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var response = await _userBL.CreateLabelAsync(labelName, userId);
                if (response.IsSuccess)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Deletes an existing label for the authenticated user
        /// </summary>
        /// <param name="labelName">Name of the label to delete</param>
        /// <returns>Success message if deleted, otherwise error message</returns>
        /// <response code="200">If the label was successfully deleted</response>
        /// <response code="400">If the label couldn't be deleted</response>
        [HttpDelete("DeleteLabel")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteLabel(string labelName)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var response = await _userBL.DeleteLabelAsync(labelName, userId);
                if (response.IsSuccess)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Associates a label with a specific note
        /// </summary>
        /// <param name="labelName">Name of the label to add</param>
        /// <param name="noteId">ID of the note to associate with</param>
        /// <returns>Response with label-note association details or error message</returns>
        /// <response code="200">Returns the label-note association details</response>
        /// <response code="400">If the association couldn't be created</response>
        [HttpPost("AddLabelToNote")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddLabelToNote(string labelName, int noteId)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var response = await _userBL.AddLabelToNoteAsync(labelName, noteId, userId);
                if (response.IsSuccess)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Retrieves all labels for the authenticated user
        /// </summary>
        /// <returns>List of all labels belonging to the user</returns>
        /// <response code="200">Returns the list of labels</response>
        /// <response code="400">If there was an error retrieving labels</response>
        [HttpGet("ViewAllLabels")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ViewAllLabels()
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var response = await _userBL.ViewAllLabelsAsync(userId);
                if (response.IsSuccess)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Updates an existing label's information
        /// </summary>
        /// <param name="request">Contains label ID and new label name</param>
        /// <returns>Updated label details or error message</returns>
        /// <response code="200">Returns the updated label details</response>
        /// <response code="400">If the label couldn't be updated</response>
        [HttpPatch("UpdateLabel")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateLabel(LabelUpdateDTO request)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var response = await _userBL.UpdateLabelAsync(request, userId);
                if (response.IsSuccess)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Retrieves a specific label by its ID
        /// </summary>
        /// <param name="labelId">ID of the label to retrieve</param>
        /// <returns>Label details if found, otherwise error message</returns>
        /// <response code="200">Returns the requested label details</response>
        /// <response code="400">If the label couldn't be found</response>
        [HttpGet("{labelId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ViewLabelById(int labelId)
        {
            try
            {
                var userId = Convert.ToInt32(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
                var response = await _userBL.GetLabelByIdAsync(labelId, userId);
                if (response.IsSuccess)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }
    }
}