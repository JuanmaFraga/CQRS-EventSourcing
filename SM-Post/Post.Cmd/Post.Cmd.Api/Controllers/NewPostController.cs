using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Cmd.Api.DTOs;
using Post.Common.DTOs;
using System.Data;

namespace Post.Cmd.Api.Controllers
{
    [ApiController]     // Marca el controller como Restfull controller
    [Route("api/v1/[controller]")]      // "[controller]" va a ser reemplazado con "NewPost", o sea "api/v1/NewPost"
    public class NewPostController : ControllerBase                 // Crea un nuevo Post sobre HTTP.
    {                                                               // Los controllers Se encargan de recibir requests HTTP, procesarlas y devolver una respuesta. No contiene lógica compleja, sólo orquesta  
        private readonly ILogger<NewPostController> _logger;        // Controller → Application → Domain → Infrastructure
        private readonly ICommandDispatcher _commandDispatcher;     // Dispatcher que representa al Mediador que se va a encargar de llamar al handler correspondiente

        public NewPostController(ILogger<NewPostController> logger, ICommandDispatcher commanDispatcher)        // Iyectamos las dependencias en el constructor
        {
            _logger = logger;
            _commandDispatcher = commanDispatcher;
        }

        [HttpPost]  // Marcamos el método del controlador como tipo POST. Se marca como un método HTTP Restfull del controller, del tipo POST. Puede tener un parámetro [HttpGet("{id}")]
        public async Task<ActionResult> NewPostAsync(NewPostCommand command)        // Toma un NewPostCommand y devuelve un ActionResult
        {
            var id = Guid.NewGuid();

            try
            {
                command.Id = id;

                await _commandDispatcher.SendAsync(command);                        // Paso el comando al dispatcher para que registre el handler correspondiente y este lo maneje

                return StatusCode(StatusCodes.Status201Created, new NewPostResponse
                {
                    Id = id,
                    Message = $"New post creation request completed succesfully!"
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
                const string SAFE_ERROR_MESSAGE = "Error while processing request to Create a New Post!";
                _logger.Log(LogLevel.Error, ex, SAFE_ERROR_MESSAGE);
                return StatusCode(StatusCodes.Status500InternalServerError, new NewPostResponse
                {
                    Id = id,
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
