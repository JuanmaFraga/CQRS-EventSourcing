using CQRS.Core.Commands;

namespace Post.Cmd.Api.Commands
{
    public class LikePostCommand : BaseCommand             // No tiene campos ni propiedades porque describe una ACCIÓN específica (dar "me gusta" a un post) que no requiere información adicional más allá del ID del post al que se le dará "me gusta". El ID del post se hereda de la clase BaseCommand, lo que permite identificar de manera única el post al que se le aplicará la acción de "me gusta".
    {
    }
}
