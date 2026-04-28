using Post.Common.DTOs;

namespace Post.Cmd.Api.DTOs
{
    public class NewPostResponse : BaseResponse
    {
        public Guid Id { get; set; }     // Propiedad que representa el identificador único del nuevo post creado. Esta propiedad se puede utilizar para devolver el ID del post recién creado en la respuesta de la API, lo que permite a los clientes identificar y acceder al nuevo post utilizando ese ID en futuras solicitudes.
    }
}
