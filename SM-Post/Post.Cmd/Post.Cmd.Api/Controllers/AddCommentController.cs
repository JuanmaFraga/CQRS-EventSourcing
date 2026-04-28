using CQRS.Core.Exeptions;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Common.DTOs;
using System.Collections.Immutable;

namespace Post.Cmd.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AddCommentController : ControllerBase              // Controller encargado de recibir el PUT (update) de añadir un comentario a un post
    {
        private readonly ILogger<AddCommentController> _logger;
        private readonly ICommandDispatcher _commandDispatcher;

        public AddCommentController(ILogger<AddCommentController> logger, ICommandDispatcher commandDispatcher)     // Constructor para inyectar las dependencias
        {
            _logger = logger;
            _commandDispatcher = commandDispatcher;
        }

        [HttpPut("{id}")]                                                               //PUT porque si bien insertamos un comment, lo hacemos sobre un recurso existente (un Post existente)
        public async Task<ActionResult> AddCommentAsync(Guid id, AddCommentCommand command)
        {
            try
            {
                command.Id = id;

                await _commandDispatcher.SendAsync(command);

                return Ok(new BaseResponse
                {
                    Message = "Add comment request completed succesfully!"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.Log(LogLevel.Warning, ex, "Client made a bad request!");
                return BadRequest(new BaseResponse                                      // BadRequest Satus 400
                {
                    Message = ex.Message,
                });
            }
            catch (AggregateNotFoundException ex)                                       // Excepcion solo para cuando editamos un post, no para cuando lo creamos por ej
            {
                _logger.Log(LogLevel.Warning, ex, "Could not retrieve aggregate, client passs an incorrect post ID targeting de aggregate!");
                return BadRequest(new BaseResponse
                {
                    Message = ex.Message,
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to Add a comment to a post!";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
