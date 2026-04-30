using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Cmd.Api.DTOs;
using Post.Common.DTOs;

namespace Post.Cmd.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RestoreReadDBController : ControllerBase                   // Controller para hacer toda la republicación de TODOS los eventos hacia KAFKA
    {
        private readonly ILogger<RestoreReadDBController> _logger;
        private readonly ICommandDispatcher _commandDispatcher;

        public RestoreReadDBController(ILogger<RestoreReadDBController> logger, ICommandDispatcher commandDispatcher)       // Inyectamos las dependencias en el constructor
        {
            _logger = logger;
            _commandDispatcher = commandDispatcher;
        }

        [HttpPost]      // POST se usa para: ejecutar acciones, disparar procesos, operaciones no puramente REST
        public async Task<ActionResult> RestoreReadDBAsync()
        {
            try
            {
                await _commandDispatcher.SendAsync(new RestoreReadDbCommand());

                return Ok(new BaseResponse
                {
                    Message = "Restore read DB command request completed succesfully!"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.Log(LogLevel.Warning, ex, "Client made a bad request!");
                return BadRequest(new BaseResponse                                          // BadRequest Satus 400
                {
                    Message = ex.Message,
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while processing request to Restpre read DB!";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
