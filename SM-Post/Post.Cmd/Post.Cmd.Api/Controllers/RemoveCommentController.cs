using CQRS.Core.Exeptions;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Common.DTOs;

namespace Post.Cmd.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RemoveCommentController : ControllerBase
    {
        private readonly ILogger<RemoveCommentController> _logger;
        private readonly ICommandDispatcher _commandDispatcher;

        public RemoveCommentController(ILogger<RemoveCommentController> logger, ICommandDispatcher commandDispatcher)       // Constructor para inyectar las dependencias
        {
            _logger = logger;
            _commandDispatcher = commandDispatcher;
        }

        [HttpDelete("{id}")]        // Acá estamos haciendo un DELETE porque estamos borrando un recurso comment (por más que en el add comment estamos haciendo un put porque modificamos un recuros -Post-)
        public async Task<ActionResult> RemoveCommentAsync(Guid id, RemoveCommentCommand command)       //Podríamos hacer todo con POST y funcionaría igual, pero usar DELETE ayuda a que otros desarrolladores (o vos mismo en el futuro) entiendan con solo mirar el log de red que esa operación fue para quitar algo del sistema.
        {
            try
            {
                command.Id = id;

                await _commandDispatcher.SendAsync(command);

                return Ok(new BaseResponse
                {
                    Message = "Remove comment request completed succesfully!"
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
                const string SAFE_ERROR_MESSAGE = "Error while processing request to Remove a comment from a post!";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
