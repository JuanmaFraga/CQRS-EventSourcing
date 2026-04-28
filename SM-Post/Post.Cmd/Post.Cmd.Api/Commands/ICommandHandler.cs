namespace Post.Cmd.Api.Commands
{
    public interface ICommandHandler                            // Interfaz que representa el Collegue en el patrón Mediator, encargada de manejar la lógica de procesamiento de comandos. Define un método genérico HandleAsync que acepta un comando de tipo T, donde T debe implementar la interfaz ICommand.
    {
        Task HandleAsync(NewPostCommand command);
        Task HandleAsync(EditMessageCommand command);
        Task HandleAsync(LikePostCommand command);
        Task HandleAsync(AddCommentCommand command);
        Task HandleAsync(EditCommentCommand command);
        Task HandleAsync(RemoveCommentCommand command);
        Task HandleAsync(DeletePostCommand command);
    }
}
