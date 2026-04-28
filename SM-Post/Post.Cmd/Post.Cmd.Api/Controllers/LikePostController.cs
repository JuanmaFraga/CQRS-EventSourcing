using CQRS.Core.Handlers;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Cmd.Api.DTOs;
using Post.Common.DTOs;

namespace Post.Cmd.Api.Controllers                                  // http://localhost:5010
{
    [ApiController]     // Marca el controller como Restfull controller
    [Route("api/v1/[controller]")]  // [controller] será reemplazado por likePost (minuscula)
    public class LikePostController : ControllerBase        //Controller Rest para darle Like a un Post existente
    {
        private readonly ILogger _logger;
        private readonly ICommandDispatcher _commandDispatcher;        // Dispatcher que representa al Mediador que se va a encargar de llamar al handler correspondiente

        public LikePostController(ILogger<LikePostController> logger, ICommandDispatcher dispatcher)        // Constructor para inyectar las dependencias del logger y el dispatcher (mediador)
        {
            _logger = logger;
            _commandDispatcher = dispatcher;
        }

        [HttpPut("{id}")]   // Updateamos
        public async Task<ActionResult> LikePostAsync(Guid id)      // LikePostCommand no tiene ningún campo específico ya que sólo aumenta el contador de likes
        {                                                           // Por eso sólo necesitamos el Id del post a Likear
            try
            {                
                await _commandDispatcher.SendAsync(new LikePostCommand { Id = id});     // Pasamos el id del post directamente

                return Ok(new BaseResponse        // Respondemos Ok 200
                {
                    Message = "Like post request completed succesfully!"
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
                const string SAFE_ERROR_MESSAGE = "Error while processing request to Like a Post!";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);
                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
