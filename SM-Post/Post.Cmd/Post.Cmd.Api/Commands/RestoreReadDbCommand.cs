using CQRS.Core.Commands;

namespace Post.Cmd.Api.Commands
{
    public class RestoreReadDbCommand : BaseCommand      // Comando para restaurar la base de lectura entera (a través de todos los eventos que estan guardados)
    {

    }
}
