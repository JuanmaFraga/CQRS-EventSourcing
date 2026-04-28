using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Common.DTOs;
using Post.Query.Api.DTOs;
using Post.Query.Api.Queries;
using Post.Query.Domain.Entities;

namespace Post.Query.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PostLookupController : ControllerBase          // Agregamos todos los métodos GET para las Querys en este único controllador
    {
        private readonly ILogger<PostLookupController> _logger;
        private readonly IQueryDispatcher<PostEntity> _queryDispatcher;

        public PostLookupController(ILogger<PostLookupController> logger, IQueryDispatcher<PostEntity> queryDispatcher)     // Constructor para inyectar las dependencias de logger y queryDispatcher
        {
            _logger = logger;
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPostAsync() 
        {
            try
            {
                var posts = await _queryDispatcher.SendAsync(new FindAllPostsQuery());      // Usamos el dispatcher va a despachar nuestro query object (FindAllPostQuery) al query handler que necesite,
                                                                                            // para que haga la consulta a través de la interfza IPostRepository
                if (posts == null || !posts.Any())
                {
                    return NoContent();     // NoContent devuelve un error 204
                }

                var count = posts.Count();  // Vamos a usar la cantidad de post para la respuesta
                
                return Ok(new PostLookupResponse
                {
                    Posts = posts,
                    Message = $"Succesfully returned {count} post{(count > 1 ? 's' : string.Empty)}!"       // Sin los () falla porque el : termina la interpolation (uso de variables dentro de {} en una string)
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while procesing request to retrieve all posts!";

                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse 
                { 
                    Message = SAFE_ERROR_MESSAGE 
                });
            }
        }

        [HttpGet("byId/{postId}")]  // Agregamos a la ruta base del controller. Quedaría "api/v1/[controller]/byId/{postId}"
        public async Task<ActionResult> GetPostByIdAsync(Guid postId)
        {
            try
            {
                var posts = await _queryDispatcher.SendAsync(new FindPostByIdQuery { Id = postId });    // Sin el await devolves una Task<List<PostEntity>>, una Task, una "promesa" de que vas a devolver una List<PostEntity>.
                                                                                                        // await dice que esperes el resultado, y ahí si devolves directamente uan List<PostEntity>
                if (posts == null || !posts.Any())
                {
                    return NoContent();     // NoContent devuelve un error 204
                }

                var count = posts.Count();  // Vamos a usar la cantidad de post para la respuesta

                return Ok(new PostLookupResponse
                {
                    Posts = posts,
                    Message = $"Succesfully returned post!"       // Sin los () falla porque el : termina la interpolation (uso de variables dentro de {} en una string)
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while procesing request to find a post by Id!";

                _logger.LogError(ex, SAFE_ERROR_MESSAGE);

                return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
                {
                    Message = SAFE_ERROR_MESSAGE
                });
            }
        }
    }
}
