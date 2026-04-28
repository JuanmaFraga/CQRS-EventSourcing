using CQRS.Core.Exeptions;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Cmd.Api.DTOs;
using Post.Common.DTOs;
using System.Runtime.Intrinsics.X86;

namespace Post.Cmd.Api.Controllers
{
    [ApiController]     // Marca el controller como Restfull controller
    [Route("api/v1/[controller]")]  // [controller] será reemplazado por editMessage (primera en minuscula)
    public class EditMessageController : ControllerBase                 // Controller REST para Editar un el Mensaje de un Post existente
    {                                                                   // Controller → Application → Domain → Infrastructure
        private readonly ILogger<EditMessageController> _logger;        
        private readonly ICommandDispatcher _commandDispatcher;         // Dispatcher que representa al Mediador que se va a encargar de llamar al handler correspondiente

        public EditMessageController(ILogger<EditMessageController> logger, ICommandDispatcher commandDispatcher)
        {
            _logger = logger;
            _commandDispatcher = commandDispatcher;
        }

        [HttpPut("{id}")]   // Updateamos
        public async Task<ActionResult> EditMessageAsync(Guid id, EditMessageCommand command)
        {
            try
            {
                command.Id = id;

                await _commandDispatcher.SendAsync(command);

                return Ok(new BaseResponse      // Respondemos Ok 200
                {
                    Message = "Edit message request completed succesfully!"
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
                const string SAFE_ERROR_MESSAGE = "Error while processing request to Edit the message of a post!";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
