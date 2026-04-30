using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Common.DTOs;
using Post.Query.Api.DTOs;
using Post.Query.Api.Queries;
using Post.Query.Domain.Entities;

namespace Post.Query.Api.Controllers                    // http://localhost:5011
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
        public async Task<ActionResult> GetAllPostAsync()
        {
            try
            {
                var posts = await _queryDispatcher.SendAsync(new FindAllPostsQuery());      // Usamos el dispatcher va a despachar nuestro query object (FindAllPostQuery) al query handler que necesite,
                                                                                            // para que haga la consulta a través de la interfza IPostRepository
                return NormalResponse(posts);
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while procesing request to retrieve all posts!";

                return ErrorResponse(ex, SAFE_ERROR_MESSAGE);
            }
        }

        [HttpGet("byId/{postId}")]  // Agregamos a la ruta base del controller. Quedaría "api/v1/[controller]/byId/{postId}"
        public async Task<ActionResult> GetPostByIdAsync(Guid postId)
        {
            try
            {
                var posts = await _queryDispatcher.SendAsync(new FindPostByIdQuery { Id = postId });    // Sin el await devolves una Task<List<PostEntity>>, una Task, una "promesa" de que vas a devolver una List<PostEntity>.
                                                                                                        // await dice que esperes el resultado, y ahí si devolves directamente uan List<PostEntity>
                if (posts == null || !posts.Any(p => p != null)!)                   // !posts.Any() No salta cuando tengo una lista de nulls. Cuando en el handler Post viene null, y hacemos List<PostEntity> { post } nos queda una lista de un null que no salta con el !posts.Any()
                {
                    return NoContent();     // NoContent devuelve un error 204
                }

                return Ok(new PostLookupResponse
                {
                    Posts = posts,
                    Message = $"Succesfully returned post!"       // Sin los () falla porque el : termina la interpolation (uso de variables dentro de {} en una string)
                });
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while procesing request to find a post by Id!";

                return ErrorResponse(ex, SAFE_ERROR_MESSAGE);
            }
        }

        [HttpGet("byAuthor/{author}")]      // En el query del PostRepository usamos x.Author.Contains(author) así que podemos pasar parte del nombre solamente
        public async Task<ActionResult> GetByAuthorAsync(string author)
        {
            try
            {
                var posts = await _queryDispatcher.SendAsync(new FindPostsByAuthorQuery { Author = author });

                return NormalResponse(posts);
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while procesing request to find posts by Author!";

                return ErrorResponse(ex, SAFE_ERROR_MESSAGE);
            }
        }

        [HttpGet("withLikes/{numberOfLikes}")]
        public async Task<ActionResult> GetWithLikesAsync(int numberOfLikes)        // En el repositorio usamos x.Likes >= numberOfLikes por lo que es con AL MENOS tantos Likes
        {
            try
            {
                var posts = await _queryDispatcher.SendAsync(new FindPostsWithLikesQuery { NumberOfLikes = numberOfLikes });

                return NormalResponse(posts);
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while procesing request to find posts with Likes!";

                return ErrorResponse(ex, SAFE_ERROR_MESSAGE);
            }
        }

        [HttpGet("withComments")]
        public async Task<ActionResult> GetWithCommentsAsync()
        {
            try
            {
                var posts = await _queryDispatcher.SendAsync(new FindPostsWithCommentsQuery());
                return NormalResponse(posts);
            }
            catch (Exception ex)
            {
                const string SAFE_ERROR_MESSAGE = "Error while procesing request to find posts with Comments!";

                return ErrorResponse(ex, SAFE_ERROR_MESSAGE);
            }
        }

        // Métodos privados
        private ActionResult NormalResponse(List<PostEntity> posts)     // Para no repetir código entre los controller y cumplir con el principio DRY (Don’t Repeat Yourself)
        {                                                               // Puede devolver Ok 200 o NoContent 204
            if (posts == null || !posts.Any(p => p != null)!)                   // !posts.Any() No salta cuando tengo una lista de nulls. Cuando en el handler Post viene null, y hacemos List<PostEntity> { post } nos queda una lista de un null que no salta con el !posts.Any()
            {
                return NoContent();     // NoContent devuelve un error status 204
            }

            var count = posts.Count();  // Vamos a usar la cantidad de post para la respuesta

            return Ok(new PostLookupResponse
            {
                Posts = posts,
                Message = $"Succesfully returned {count} post{(count > 1 ? 's' : string.Empty)}!"       // Sin los () falla porque el : termina la interpolation (uso de variables dentro de {} en una string)
            });
        }

        private ActionResult ErrorResponse(Exception ex, string SAFE_ERROR_MESSAGE)     // Para no repetir código entre los controller y cumplir con el principio DRY (Don’t Repeat Yourself)
        {                                                                               // Responde Error 500
            _logger.LogError(ex, SAFE_ERROR_MESSAGE);

            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
            {
                Message = SAFE_ERROR_MESSAGE
            });
        }
    }
}
