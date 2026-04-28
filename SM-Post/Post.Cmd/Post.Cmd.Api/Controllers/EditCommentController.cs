using CQRS.Core.Exeptions;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Common.DTOs;

namespace Post.Cmd.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EditCommentController : ControllerBase
    {
        private readonly ILogger<EditCommentCommand> _logger;
        private readonly ICommandDispatcher _commandDispatcher;

        public EditCommentController(ILogger<EditCommentCommand> logger, ICommandDispatcher commandDispatcher)
        {
            _logger = logger;
            _commandDispatcher = commandDispatcher;
        }

        [HttpPut("{id}")]   // El Id que pasamos el es el de la entidad PRINCIPAL (el POST)!!
        public async Task<ActionResult> EditCommentAsync(Guid id, EditCommentCommand command)   // El Id que pasamos el es el de la entidad PRINCIPAL (el POST)!!
        {
            try
            {
                command.Id = id;    // El Id que pasamos el es el de la entidad PRINCIPAL (el POST)!!

                await _commandDispatcher.SendAsync(command);
                return Ok(new BaseResponse
                {
                    Message = "Edit comment request completed succesfully!"
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
                const string SAFE_ERROR_MESSAGE = "Error while processing request to Edit a comment from a post!";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
